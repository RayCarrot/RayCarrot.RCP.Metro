using System;
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
public class Page_GamesNew_ViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    public Page_GamesNew_ViewModel(
        AppViewModel app, 
        GamesManager gamesManager, 
        AppUIManager ui, 
        IMessenger messenger) : base(app)
    {
        // Set services
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        // Set properties
        AsyncLock = new AsyncLock();

        GameCategories = new ObservableCollection<InstalledGameCategoryViewModel>();
        var source = CollectionViewSource.GetDefaultView(GameCategories);
        source.Filter = p => !((InstalledGameCategoryViewModel)p).FilteredGameGroups.IsEmpty;
        FilteredGameCategories = source;

        // Create commands
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);
        AddGamesCommand = new AsyncRelayCommand(AddGamesAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RefreshGamesCommand { get; }
    public ICommand AddGamesCommand { get; }

    #endregion

    #region Private Fields

    private string _gameFilter = String.Empty;

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Services

    private GamesManager GamesManager { get; }
    private AppUIManager UI { get; }
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

    public Task AddGamesAsync()
    {
        return UI.ShowAddGamesAsync();
    }

    public async void Receive(AddedGamesMessage message) =>
        await RefreshAsync(message.GameInstallations.FirstOrDefault());
    public async void Receive(RemovedGamesMessage message) =>
        await RefreshAsync(SelectedInstalledGame?.GameInstallation);
    // NOTE: We could optimize this by just refreshing the game which has been modified, but atm it's not worth it
    public async void Receive(ModifiedGamesMessage message) =>
        await RefreshAsync(SelectedInstalledGame?.GameInstallation);

    #endregion
}