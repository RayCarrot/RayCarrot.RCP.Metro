using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the games page
/// </summary>
public class Page_Games_ViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    public Page_Games_ViewModel(
        AppViewModel app,
        AppUserData data,
        GamesManager gamesManager, 
        AppUIManager ui,
        IMessageUIManager messageUi,
        IMessenger messenger) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        // Set properties
        AsyncLock = new AsyncLock();

        GameCategories = new ObservableCollection<InstalledGameCategoryViewModel>();
        var source = CollectionViewSource.GetDefaultView(GameCategories);
        source.Filter = p => !((InstalledGameCategoryViewModel)p).FilteredGameGroups.IsEmpty;
        FilteredGameCategories = source;

        // Create commands
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);
        FindGamesCommand = new AsyncRelayCommand(() => FindGamesAsync(false));
        AddGamesCommand = new AsyncRelayCommand(AddGamesAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RefreshGamesCommand { get; }
    public ICommand FindGamesCommand { get; }
    public ICommand AddGamesCommand { get; }

    #endregion

    #region Private Fields

    private string _gameFilter = String.Empty;

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    private AppUserData Data { get; }
    private GamesManager GamesManager { get; }
    private AppUIManager UI { get; }
    private IMessageUIManager MessageUI { get; }
    private IMessenger Messenger { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Games;

    public string GameFilter
    {
        get => _gameFilter;
        set
        {
            _gameFilter = value;
            UpdateFilteredCollections();
        }
    }

    public ObservableCollection<InstalledGameCategoryViewModel> GameCategories { get; }
    public ICollectionView FilteredGameCategories { get; }

    public InstalledGameViewModel? SelectedInstalledGame { get; set; }

    public bool IsGameFinderRunning { get; private set; }

    #endregion

    #region Protected Methods

    protected override async Task InitializeAsync()
    {
        await RefreshAsync();

        // Register for messages
        Messenger.RegisterAll(this);
    }

    #endregion

    #region Public Methods

    public void UpdateFilteredCollections()
    {
        foreach (InstalledGameCategoryViewModel gameCategory in GameCategories)
            gameCategory.UpdateFilteredCollections();

        FilteredGameCategories.Refresh();
    }

    public Task RefreshAsync() => RefreshAsync(null);

    public async Task RefreshAsync(GameInstallation? selectedGameInstallation)
    {
        using (await AsyncLock.LockAsync())
        {
            Metro.App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Clear the categories
                    GameCategories.Clear();

                    SelectedInstalledGame = null;

                    // Enumerate every category of installed games
                    foreach (var categorizedGames in GamesManager.EnumerateInstalledGames().
                                 OrderBy(x => x.GameDescriptor.Category). // TODO-14: Normalize games sorting
                                 GroupBy(x => x.GameDescriptor.Category))
                    {
                        // Create a view model
                        GameCategoryInfoAttribute categoryInfo = categorizedGames.Key.GetInfo();
                        InstalledGameCategoryViewModel category = new(categoryInfo.DisplayName, () => GameFilter);
                        GameCategories.Add(category);

                        // Enumerate every group of installed games
                        foreach (var gameInstallations in categorizedGames.
                                     OrderBy(x => x.GameDescriptor.Game). // TODO-14: Normalize games sorting
                                     GroupBy(x => x.GameDescriptor.Game))
                        {
                            // Get the game info
                            GameInfoAttribute gameInfo = gameInstallations.Key.GetInfo();

                            InstalledGameGroupViewModel group = new(
                                // TODO-UPDATE: Normalize
                                iconSource: $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{gameInfo.GameIcon.GetAttribute<ImageFileAttribute>()!.FileName}",
                                displayName: gameInfo.DisplayName,
                                // TODO-14: This needs to be sorted somehow. Either by user or by game descr.
                                gameInstallations: gameInstallations);

                            // Add the group of game installations
                            category.GameGroups.Add(group);

                            if (selectedGameInstallation != null)
                            {
                                InstalledGameViewModel? selectedInstalledGame = group.InstalledGames.
                                    FirstOrDefault(x => x.GameInstallation == selectedGameInstallation);

                                if (selectedInstalledGame != null)
                                {
                                    group.SelectedInstalledGame = selectedInstalledGame;
                                    selectedGameInstallation = null;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO-UPDATE: Handle exception
                    Logger.Fatal(ex, "Refreshing games");
                    throw;
                }
                finally
                {
                    UpdateFilteredCollections();
                }
            });
        }
    }

    public async Task RefreshInstallations(IList<GameInstallation> gameInstallations)
    {
        using (await AsyncLock.LockAsync())
        {
            foreach (InstalledGameCategoryViewModel gameCategory in GameCategories)
            {
                foreach (InstalledGameGroupViewModel gameGroup in gameCategory.GameGroups)
                {
                    foreach (InstalledGameViewModel installedGame in gameGroup.InstalledGames)
                    {
                        if (gameInstallations.Contains(installedGame.GameInstallation))
                        {
                            await installedGame.RefreshAsync();
                        }
                    }
                }
            }
        }
    }

    public async Task FindGamesAsync(bool runInBackground)
    {
        if (IsGameFinderRunning)
            return;

        IsGameFinderRunning = true;

        IReadOnlyList<GameFinder_BaseResult> foundItems;

        try
        {
            // TODO-14: Change how the game finder works
            // Get all games which have not been added
            GameDescriptor[] games = Services.Games.EnumerateGameDescriptors().
                Where(x => Services.Games.EnumerateInstalledGames().All(g => g.GameDescriptor != x)).
                ToArray();

            Logger.Trace("The following games were added to the game checker: {0}", games.JoinItems(", "));

            // Get additional finder items
            List<GameFinder_GenericItem> finderItems = new(1);

            // Create DOSBox finder item if it doesn't exist
            if (!System.IO.File.Exists(Data.Emu_DOSBox_Path))
            {
                string[] names =
                {
                    "DosBox",
                    "Dos Box"
                };

                void foundAction(FileSystemPath installDir)
                {
                    if (System.IO.File.Exists(Data.Emu_DOSBox_Path))
                    {
                        Logger.Warn("The DosBox executable was not added from the game finder due to already having been added");
                        return;
                    }

                    Logger.Info("The DosBox executable was found from the game finder");

                    Data.Emu_DOSBox_Path = installDir + "DOSBox.exe";
                }

                finderItems.Add(new GameFinder_GenericItem(names, "DosBox", x => (x + "DOSBox.exe").FileExists ? x : (FileSystemPath?)null, foundAction, "DOSBox"));
            }

            // Run the game finder and get the result
            GameFinder finder = new(games, finderItems);
            foundItems = await Task.Run(finder.FindGames);

            // Handle the generic results
            foreach (GameFinder_GenericResult genericResult in foundItems.OfType<GameFinder_GenericResult>())
                genericResult.HandledAction?.Invoke(genericResult.InstallLocation);

            // Handle the game results by adding the found games
            if (foundItems.OfType<GameFinder_GameResult>().Any())
                await GamesManager.AddGamesAsync(foundItems.OfType<GameFinder_GameResult>().
                    Select(x => (x.GameDescriptor, x.InstallLocation)));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Game finder");
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameFinder_Error);
            return;
        }
        finally
        {
            IsGameFinderRunning = false;
        }

        // Check if new games were found
        if (foundItems.Count > 0)
        {
            // Split into found games and items and sort
            IEnumerable<string> gameFinderResults = foundItems.
                OfType<GameFinder_GameResult>().
                OrderBy(x => x.GameDescriptor.Game).
                Select(x => x.DisplayName);

            IEnumerable<string> finderResults = foundItems.
                OfType<GameFinder_GenericResult>().
                OrderBy(x => x.DisplayName).
                Select(x => x.DisplayName);

            await MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {gameFinderResults.Concat(finderResults).JoinItems(Environment.NewLine + "• ")}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);

            Logger.Info("The game finder found the following items {0}", foundItems.JoinItems(", ", x => x.DisplayName));
        }
        else if (!runInBackground)
        {
            await MessageUI.DisplayMessageAsync(Resources.GameFinder_NoResults, Resources.GameFinder_ResultHeader,
                MessageType.Information);
        }
    }

    public Task AddGamesAsync()
    {
        return UI.ShowAddGamesAsync();
    }

    public async void Receive(AddedGamesMessage message) =>
        await RefreshAsync(message.GameInstallations.FirstOrDefault());
    public async void Receive(RemovedGamesMessage message) =>
        await RefreshAsync(SelectedInstalledGame?.GameInstallation);
    public async void Receive(ModifiedGamesMessage message) =>
        await RefreshInstallations(message.GameInstallations);

    #endregion
}