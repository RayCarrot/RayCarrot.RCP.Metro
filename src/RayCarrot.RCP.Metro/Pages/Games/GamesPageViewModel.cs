using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Pages.Games;

/// <summary>
/// View model for the games page
/// </summary>
public class GamesPageViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>,
    IRecipient<SortedGamesMessage>
{
    #region Constructor

    public GamesPageViewModel(
        AppViewModel app,
        GamesManager gamesManager, 
        GameClientsManager gameClientsManager, 
        AppUIManager ui,
        IMessageUIManager messageUi,
        IMessenger messenger, 
        AppUserData data) : base(app)
    {
        // Set services
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        GameClientsManager = gameClientsManager ?? throw new ArgumentNullException(nameof(gameClientsManager));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        Data = data ?? throw new ArgumentNullException(nameof(data));

        // Set properties
        AsyncLock = new AsyncLock();

        // Set up the games collection
        Games = new ObservableCollectionEx<InstalledGameViewModel>();
        GamesView = CollectionViewSource.GetDefaultView(Games);
        GamesView.Filter = x => MatchesFilter((InstalledGameViewModel)x);

        RefreshGrouping(GroupGames);

        // Create commands
        DeselectGameCommand = new RelayCommand(DeselectGame);
        SelectGameCommand = new RelayCommand(x => SelectGame((InstalledGameViewModel)x!));
        ResetSortCommand = new RelayCommand(ResetSort);
        RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);
        FindGamesCommand = new AsyncRelayCommand(() => FindGamesAsync(false));
        AddGamesCommand = new AsyncRelayCommand(AddGamesAsync);
        ConfigureGameClientsCommand = new AsyncRelayCommand(ConfigureGameClientsAsync);
        ShowVersionHistoryCommand = new AsyncRelayCommand(ShowVersionHistoryAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand DeselectGameCommand { get; }
    public ICommand SelectGameCommand { get; }
    public ICommand ResetSortCommand { get; }
    public ICommand RefreshGamesCommand { get; }
    public ICommand FindGamesCommand { get; }
    public ICommand AddGamesCommand { get; }
    public ICommand ConfigureGameClientsCommand { get; }
    public ICommand ShowVersionHistoryCommand { get; }

    #endregion

    #region Private Fields

    private string _gameFilter = String.Empty;
    private InstalledGameViewModel? _selectedInstalledGame;
    private bool _isSettingGroupSelection;
    private readonly Dictionary<Game, GameGroupViewModel> _gamegroups = new();

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
    private AppUserData Data { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Games;

    public string GameFilter
    {
        get => _gameFilter;
        set
        {
            _gameFilter = value;
            GamesView.Refresh();
        }
    }

    public ObservableCollectionEx<InstalledGameViewModel> Games { get; }
    public ICollectionView GamesView { get; }
    public ObservableCollectionEx<InstalledGameViewModel>? RecentGames { get; set; }
    public ObservableCollectionEx<InstalledGameViewModel>? FavoriteGames { get; set; }

    public InstalledGameViewModel? SelectedInstalledGame
    {
        get => _selectedInstalledGame;
        set
        {
            _isSettingGroupSelection = true;

            try
            {
                if (value != null)
                {
                    foreach (GameGroupViewModel group in _gamegroups.Values)
                        group.IsSelected = value.GameGroup == group;
                }
                else
                {
                    foreach (GameGroupViewModel group in _gamegroups.Values)
                        group.IsSelected = false;
                }
            }
            finally
            {
                _isSettingGroupSelection = false;
            }

            SetSelectedGame(value);
        }
    }

    public bool IsGameFinderRunning { get; private set; }

    public bool GroupGames
    {
        get => Data.UI_GroupInstalledGames;
        set
        {
            Data.UI_GroupInstalledGames = value;

            // Sort the games to match the grouping. Otherwise the order
            // in the ui doesn't match the actual order and moving the
            // games around won't work as expected
            GamesManager.SortGames((x, y) => x.GameDescriptor.Game.CompareTo(y.GameDescriptor.Game));
            
            RefreshGrouping(value);
        }
    }

    #endregion

    #region Private Methods

    private bool MatchesFilter(InstalledGameViewModel game)
    {
        bool matchesString(string str) => str.IndexOf(GameFilter, StringComparison.CurrentCultureIgnoreCase) != -1;

        return matchesString(game.DisplayName) ||
               game.GameDescriptor.SearchKeywords.Any(matchesString);
    }

    private async void SetSelectedGame(InstalledGameViewModel? game)
    {
        _selectedInstalledGame = game;
        OnPropertyChanged(nameof(SelectedInstalledGame));

        if (game != null)
            await game.LoadAsync();
    }

    private void GameGroup_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_isSettingGroupSelection)
            return;

        // Handle selecting the group
        if (e.PropertyName == nameof(GameGroupViewModel.IsSelected) &&
            sender is GameGroupViewModel { IsSelected: true } group)
        {
            // Deselect all other groups
            foreach (GameGroupViewModel g in _gamegroups.Values.Where(x => x != group))
                g.IsSelected = false;

            // Select first game in group
            if (SelectedInstalledGame?.GameGroup != group)
                SetSelectedGame(Games.FirstOrDefault(x => x.GameGroup == group));
        }
    }

    private void RefreshGrouping(bool group)
    {
        if (group && GamesView.GroupDescriptions.Count == 0)
            GamesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(InstalledGameViewModel.GameGroup)));
        else
            GamesView.GroupDescriptions.Clear();
    }

    private void RefreshRecentGames()
    {
        RecentGames = new ObservableCollectionEx<InstalledGameViewModel>(Games.
            OrderByDescending(x =>
            {
                DateTime lastPlayed = x.GameInstallation.GetValue<DateTime>(GameDataKey.RCP_LastPlayed);
                DateTime gameAdded = x.GameInstallation.GetValue<DateTime>(GameDataKey.RCP_GameAddedDate);

                // Use the latest date of the two
                return lastPlayed > gameAdded ? lastPlayed : gameAdded;
            }).
            Take(5));
    }

    private void RefreshFavoriteGames()
    {
        FavoriteGames = new ObservableCollectionEx<InstalledGameViewModel>(Games.
            Where(x => x.GameInstallation.GetValue<bool>(GameDataKey.RCP_IsFavorite)));
    }

    private void Game_OnLastPlayedChanged() => RefreshRecentGames();
    private void Game_OnIsFavoriteChanged() => RefreshFavoriteGames();

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

    public void DeselectGame() => SelectedInstalledGame = null;
    public void SelectGame(InstalledGameViewModel game) => SelectedInstalledGame = game;
    public void ResetSort() => GamesManager.SortGames((x, y) => x.CompareTo(y));

    public Task RefreshAsync() => RefreshAsync(null);

    public async Task RefreshAsync(GameInstallation? selectedGameInstallation)
    {
        using (await AsyncLock.LockAsync())
        {
            Metro.App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Unload previously loaded games
                    foreach (InstalledGameViewModel game in Games)
                    {
                        game.GameInstallation.RemoveDataChangedCallback(GameDataKey.RCP_LastPlayed, Game_OnLastPlayedChanged);
                        game.GameInstallation.RemoveDataChangedCallback(GameDataKey.RCP_IsFavorite, Game_OnIsFavoriteChanged);
                        game.Unload();
                    }

                    foreach (GameGroupViewModel group in _gamegroups.Values)
                        group.PropertyChanged -= GameGroup_OnPropertyChanged;

                    SelectedInstalledGame = null;

                    Games.ModifyCollection(x =>
                    {
                        // Clear the games
                        x.Clear();
                        _gamegroups.Clear();

                        // Enumerate every group of installed games
                        foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
                        {
                            Game game = gameInstallation.GameDescriptor.Game;

                            if (!_gamegroups.TryGetValue(game, out GameGroupViewModel group))
                            {
                                // Create a view model for the group
                                GameInfoAttribute gameInfo = game.GetInfo();
                                group = new GameGroupViewModel(gameInfo.GameIcon, gameInfo.DisplayName);
                                group.PropertyChanged += GameGroup_OnPropertyChanged;

                                _gamegroups[game] = group;
                            }

                            x.Add(new InstalledGameViewModel(gameInstallation, group));
                        }
                    });

                    RefreshRecentGames();
                    RefreshFavoriteGames();

                    foreach (InstalledGameViewModel game in Games)
                    {
                        game.GameInstallation.AddDataChangedCallback(GameDataKey.RCP_LastPlayed, Game_OnLastPlayedChanged);
                        game.GameInstallation.AddDataChangedCallback(GameDataKey.RCP_IsFavorite, Game_OnIsFavoriteChanged);
                    }

                    if (selectedGameInstallation != null)
                        SelectedInstalledGame = Games.FirstOrDefault(x => x.GameInstallation == selectedGameInstallation);
                }
                catch (Exception ex)
                {
                    // TODO-UPDATE: Handle exception
                    Logger.Fatal(ex, "Refreshing games");
                    throw;
                }
                finally
                {
                    GamesView.Refresh();
                }
            });
        }
    }

    public async Task RefreshInstallations(IList<GameInstallation> gameInstallations, bool rebuiltComponents)
    {
        using (await AsyncLock.LockAsync())
        {
            foreach (InstalledGameViewModel installedGame in Games)
            {
                if (gameInstallations.Contains(installedGame.GameInstallation))
                {
                    await installedGame.RefreshAsync(rebuiltComponents);
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
            foreach (GameClientDescriptor gameClientDescriptor in GameClientsManager.GetGameClientDescriptors())
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

    public Task AddGamesAsync() => UI.ShowAddGamesAsync();
    public Task ConfigureGameClientsAsync() => UI.ShowGameClientsSetupAsync();
    public Task ShowVersionHistoryAsync() => UI.ShowAppNewsAsync();

    public async void Receive(AddedGamesMessage message) =>
        await RefreshAsync(message.GameInstallations.FirstOrDefault());
    public async void Receive(RemovedGamesMessage message) =>
        await RefreshAsync(SelectedInstalledGame?.GameInstallation);
    public async void Receive(ModifiedGamesMessage message) =>
        await RefreshInstallations(message.GameInstallations, message.RebuiltComponents);
    public void Receive(SortedGamesMessage message)
    {
        Games.ModifyCollection(x => 
            x.Sort((x1, x2) => message.SortedCollection.
                IndexOf(x1.GameInstallation).
                CompareTo(message.SortedCollection.
                    IndexOf(x2.GameInstallation))));
        FavoriteGames?.ModifyCollection(x => 
            x.Sort((x1, x2) => message.SortedCollection.
                IndexOf(x1.GameInstallation).
                CompareTo(message.SortedCollection.
                    IndexOf(x2.GameInstallation))));
    }

    #endregion
}