using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Finder;

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
        GamesManager gamesManager, 
        GameClientsManager gameClientsManager, 
        AppUIManager ui,
        IMessageUIManager messageUi,
        IMessenger messenger) : base(app)
    {
        // Set services
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        GameClientsManager = gameClientsManager ?? throw new ArgumentNullException(nameof(gameClientsManager));
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
        ConfigureGameClientsCommand = new AsyncRelayCommand(ConfigureGameClientsAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RefreshGamesCommand { get; }
    public ICommand FindGamesCommand { get; }
    public ICommand AddGamesCommand { get; }
    public ICommand ConfigureGameClientsCommand { get; }

    #endregion

    #region Private Fields

    private string _gameFilter = String.Empty;

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    private GamesManager GamesManager { get; }
    private GameClientsManager GameClientsManager { get; }
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
                    foreach (var categorizedGames in GamesManager.GetInstalledGames().
                                 GroupBy(x => x.GameDescriptor.Category))
                    {
                        // Create a view model
                        GameCategoryInfoAttribute categoryInfo = categorizedGames.Key.GetInfo();
                        InstalledGameCategoryViewModel category = new(categoryInfo.DisplayName, () => GameFilter);
                        GameCategories.Add(category);

                        // Enumerate every group of installed games
                        foreach (var gameInstallations in categorizedGames.GroupBy(x => x.GameDescriptor.Game))
                        {
                            // Get the game info
                            GameInfoAttribute gameInfo = gameInstallations.Key.GetInfo();

                            InstalledGameGroupViewModel group = new(
                                icon: gameInfo.GameIcon,
                                displayName: gameInfo.DisplayName,
                                gameInstallations: gameInstallations);

                            // Add the group of game installations
                            category.GameGroups.Add(group);

                            if (selectedGameInstallation != null)
                            {
                                InstalledGameViewModel? selectedInstalledGame = group.InstalledGames.
                                    // ReSharper disable once AccessToModifiedClosure
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

    public async Task RefreshInstallations(IList<GameInstallation> gameInstallations, bool rebuiltComponents)
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
                            await installedGame.RefreshAsync(rebuiltComponents);
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

        FinderItem[] runFinderItems;

        try
        {
            // Get the installed games
            IReadOnlyList<GameClientInstallation> installedGameClients = GameClientsManager.GetInstalledGameClients();
            IReadOnlyList<GameInstallation> installedGames = GamesManager.GetInstalledGames();

            // Add the items for game clients and games
            List<FinderItem> finderItems = new();

            // Get finder items for all game clients which don't have an added game client installation
            foreach (GameClientDescriptor gameClientDescriptor in GameClientsManager.GetGameCientDescriptors())
            {
                // Make sure the game client has not already been added
                if (installedGameClients.Any(g => g.GameClientDescriptor == gameClientDescriptor))
                    continue;

                // Get the finder item for the game client
                GameClientFinderItem? finderItem = gameClientDescriptor.GetFinderItem();

                if (finderItem == null)
                    continue;

                finderItems.Add(finderItem);
            }

            // Get finder items for all games which don't have an added game installation
            foreach (GameDescriptor gameDescriptor in GamesManager.GetGameDescriptors())
            {
                // Make sure the game has not already been added
                if (installedGames.Any(g => g.GameDescriptor == gameDescriptor))
                    continue;

                // Get the finder item for the game
                GameFinderItem? finderItem = gameDescriptor.GetFinderItem();

                if (finderItem == null)
                    continue;

                finderItems.Add(finderItem);
            }

            // Create a finder
            Finder finder = new(Finder.DefaultOperations, finderItems.ToArray());

            // Run the finder
            await Task.Run(finder.Run);

            // Get the finder items
            runFinderItems = finder.FinderItems;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Running finder");
            await MessageUI.DisplayExceptionMessageAsync(ex, 
                // TODO-UPDATE: Update localization (and other occurrences of "Game finder" or "Find games")
                Resources.GameFinder_Error);
            return;
        }
        finally
        {
            IsGameFinderRunning = false;
        }

        bool foundItems = false;

        // Add the found game clients
        IList<GameClientInstallation> addedGameClients = await GameClientsManager.AddGameClientsAsync(runFinderItems.OfType<GameClientFinderItem>().
            Where(x => x.HasBeenFound).
            Select(x => (x.GameClientDescriptor, x.FoundLocation!.Value)));

        if (addedGameClients.Any())
        {
            foundItems = true;

            Logger.Info("The finder found {0} game clients", addedGameClients.Count);

            // TODO-UPDATE: Localize
            await MessageUI.DisplayMessageAsync($"The following new game clients/emulators were found:{Environment.NewLine}{Environment.NewLine}• {addedGameClients.Select(x => x.GetDisplayName()).JoinItems(Environment.NewLine + "• ")}", "Installed game clients/emulators found", MessageType.Success);
        }

        // Add the found games
        IList<GameInstallation> addedGames = await GamesManager.AddGamesAsync(runFinderItems.OfType<GameFinderItem>().
            Where(x => x.HasBeenFound).
            Select(x => (x.GameDescriptor, x.FoundLocation!.Value)));

        if (addedGames.Any())
        {
            foundItems = true;

            Logger.Info("The finder found {0} games", addedGames.Count);

            await MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {addedGames.Select(x => x.GetDisplayName()).JoinItems(Environment.NewLine + "• ")}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);
        }

        if (!foundItems && !runInBackground)
            // TODO-UPDATE: Update localization to say games or game clients?
            await MessageUI.DisplayMessageAsync(Resources.GameFinder_NoResults, Resources.GameFinder_ResultHeader,
                MessageType.Information);
    }

    public Task AddGamesAsync()
    {
        return UI.ShowAddGamesAsync();
    }

    public Task ConfigureGameClientsAsync()
    {
        return UI.ShowGameClientsSetupAsync();
    }

    public async void Receive(AddedGamesMessage message) =>
        await RefreshAsync(message.GameInstallations.FirstOrDefault());
    public async void Receive(RemovedGamesMessage message) =>
        await RefreshAsync(SelectedInstalledGame?.GameInstallation);
    public async void Receive(ModifiedGamesMessage message) =>
        await RefreshInstallations(message.GameInstallations, message.RebuiltComponents);

    #endregion
}