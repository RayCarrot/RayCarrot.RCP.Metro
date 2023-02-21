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
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>
{
    #region Constructor

    public GamesPageViewModel(
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

        // Set up the games collection
        Games = new ObservableCollection<InstalledGameViewModel>();
        var source = CollectionViewSource.GetDefaultView(Games);
        source.GroupDescriptions.Add(new PropertyGroupDescription(nameof(InstalledGameViewModel.GameGroup)));
        source.Filter = p => MatchesFilter((InstalledGameViewModel)p);
        GamesView = source;

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
    private InstalledGameViewModel? _selectedInstalledGame;
    private bool _isSettingGroupSelection;
    private readonly List<GameGroupViewModel> _gamegroups = new();

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
            GamesView.Refresh();
        }
    }

    public ObservableCollection<InstalledGameViewModel> Games { get; }
    public ICollectionView GamesView { get; }

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
                    foreach (GameGroupViewModel group in _gamegroups)
                        group.IsSelected = value.GameGroup == group;
                }
                else
                {
                    foreach (GameGroupViewModel group in _gamegroups)
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
            foreach (GameGroupViewModel g in _gamegroups.Where(x => x != group))
                g.IsSelected = false;

            // Select first game in group
            if (SelectedInstalledGame?.GameGroup != group)
                SetSelectedGame(Games.FirstOrDefault(x => x.GameGroup == group));
        }
    }

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
                        game.Unload();

                    foreach (GameGroupViewModel group in _gamegroups)
                        group.PropertyChanged -= GameGroup_OnPropertyChanged;

                    // Clear the games
                    Games.Clear();
                    _gamegroups.Clear();

                    SelectedInstalledGame = null;

                    // Enumerate every group of installed games
                    foreach (var gameInstallations in GamesManager.GetInstalledGames().GroupBy(x => x.GameDescriptor.Game))
                    {
                        // Get the game info
                        GameInfoAttribute gameInfo = gameInstallations.Key.GetInfo();

                        // Create a view model for the group
                        GameGroupViewModel group = new(
                            icon: gameInfo.GameIcon,
                            displayName: gameInfo.DisplayName);

                        group.PropertyChanged += GameGroup_OnPropertyChanged;
                        _gamegroups.Add(group);

                        // Add the games
                        foreach (GameInstallation gameInstallation in gameInstallations)
                            Games.Add(new InstalledGameViewModel(gameInstallation, group));
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