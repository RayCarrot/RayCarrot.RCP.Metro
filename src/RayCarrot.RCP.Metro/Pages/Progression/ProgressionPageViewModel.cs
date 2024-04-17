using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Pages.Progression;

public class ProgressionPageViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>, 
    IRecipient<SortedGamesMessage>, IRecipient<BackupLocationChangedMessage>,
    IRecipient<ModifiedGameProgressionMessage>
{
    #region Constructor

    public ProgressionPageViewModel(
        AppViewModel app, 
        AppUserData data, 
        IMessageUIManager messageUi, 
        GamesManager gamesManager, 
        IMessenger messenger) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        // Create properties
        AsyncLock = new AsyncLock();
        Games = new ObservableCollectionEx<GameViewModel>();
        GamesView = CollectionViewSource.GetDefaultView(Games);
        RefreshFiltering(true);

        RefreshGrouping(GroupGames);

        // Create commands
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        EditVisibilityCommand = new RelayCommand(EditVisibility);
        SaveEditVisibilityCommand = new RelayCommand(SaveEditVisibility);
        BackupAllCommand = new AsyncRelayCommand(BackupAllAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand EditVisibilityCommand { get; }
    public ICommand SaveEditVisibilityCommand { get; }
    public ICommand BackupAllCommand { get; }

    #endregion

    #region Services

    public AppUserData Data { get; } // Need to keep public for now due to binding
    private IMessageUIManager MessageUI { get; }
    private GamesManager GamesManager { get; }
    private IMessenger Messenger { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Progression;
    public ObservableCollectionEx<GameViewModel> Games { get; }
    public ICollectionView GamesView { get; }

    public bool IsEditingVisibility { get; set; }

    public bool GroupGames
    {
        get => Data.UI_GroupProgressionGames;
        set
        {
            Data.UI_GroupProgressionGames = value;
            RefreshGrouping(value);
        }
    }

    #endregion

    #region Private Methods

    private void RefreshGrouping(bool group)
    {
        if (group && GamesView.GroupDescriptions.Count == 0)
            GamesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(GameViewModel.GameGroup)));
        else
            GamesView.GroupDescriptions.Clear();

        foreach (GameViewModel game in Games)
            game.IsGameGrouped = group;
    }

    private void RefreshFiltering(bool filter)
    {
        if (filter)
            GamesView.Filter = x => ((GameViewModel)x).GameInstallation.GetValue<bool>(GameDataKey.Progression_Show, true);
        else
            GamesView.Filter = null;

        GamesView.Refresh();
    }

    #endregion

    #region Protected Methods

    protected override Task InitializeAsync()
    {
        Messenger.RegisterAll(this);

        return RefreshAsync();
    }

    #endregion

    #region Public Methods

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                Logger.Info("Refreshing progression game items");

                SaveEditVisibility();

                Metro.App.Current.Dispatcher.Invoke(() =>
                {
                    Games.ModifyCollection(x =>
                    {
                        x.Clear();

                        Dictionary<Game, GameGroupViewModel> groups = new();

                        // Add the game items
                        foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
                        {
                            Game game = gameInstallation.GameDescriptor.Game;

                            if (!groups.TryGetValue(game, out GameGroupViewModel group))
                            {
                                // Create a view model for the group
                                GameInfoAttribute gameInfo = game.GetInfo();
                                group = new GameGroupViewModel(gameInfo.GameIcon, gameInfo.DisplayName);

                                groups[game] = group;
                            }

                            x.AddRange(gameInstallation.
                                GetComponents<ProgressionManagersComponent>().
                                CreateManyObjects().
                                Select(progressionManager => new GameViewModel(progressionManager, group)));
                        }
                    });
                });

                // Group games based on the backup id. Games which share the same id also share the same backup and
                // the ui should specify this to avoid confusion.
                Dictionary<string, List<GameViewModel>> backupsById = Games.
                    GroupBy(x => x.ProgressionManager.ProgressionId).
                    ToDictionary(x => x.Key, x => x.ToList());

                // Set linked games
                foreach (GameViewModel game in Games)
                {
                    List<GameViewModel> linkedGames = backupsById[game.ProgressionManager.ProgressionId];
                    game.LinkedGames = new ObservableCollection<GameViewModel>(linkedGames.Where(x => x != game));
                }

                // TODO: Use Task.WhenAll and run in parallel?

                // Load the game items
                foreach (GameViewModel game in Games)
                    await game.LoadProgressAsync();

                // Load backups
                foreach (GameViewModel game in Games)
                    await game.LoadBackupAsync();

                // Load slot infos
                foreach (GameViewModel game in Games)
                    await game.LoadSlotInfoItemsAsync();

                Logger.Info("Refreshed progression game items");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing progression");
                throw;
            }
        }
    }

    public void EditVisibility()
    {
        IsEditingVisibility = true;

        foreach (GameViewModel game in Games)
        {
            game.IsVisibleEdit = game.GameInstallation.GetValue<bool>(GameDataKey.Progression_Show, true);
            game.IsEditingVisibility = true;
        }

        RefreshFiltering(false);
    }

    public void SaveEditVisibility()
    {
        if (!IsEditingVisibility)
            return;

        IsEditingVisibility = false;

        foreach (GameViewModel game in Games)
        {
            game.GameInstallation.SetValue<bool>(GameDataKey.Progression_Show, game.IsVisibleEdit);
            game.IsEditingVisibility = false;
        }

        RefreshFiltering(true);
    }

    public async Task BackupAllAsync()
    {
        // Make sure no backups are running
        if (Games.Any(x => x.IsPerformingBackupRestore))
            return;

        // Lock
        using (await AsyncLock.LockAsync())
        {
            // Confirm backup
            if (!await MessageUI.DisplayMessageAsync(Resources.Backup_ConfirmBackupAll, Resources.Backup_ConfirmBackupAllHeader, MessageType.Warning, true))
            {
                Logger.Info("Backup canceled");

                return;
            }

            int completed = 0;

            // Perform a backup for each id
            HashSet<string> ids = new();
            foreach (GameViewModel game in Games)
            {
                if (ids.Contains(game.ProgressionManager.ProgressionId))
                    continue;

                ids.Add(game.ProgressionManager.ProgressionId);

                if (await game.BackupAsync(true))
                    completed++;
            }

            if (completed == Games.Count)
                await MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Backup_BackupAllSuccess);
            else
                await MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_BackupAllFailed, completed, Games.Count), Resources.Backup_BackupAllFailedHeader, MessageType.Information);
        }
    }

    #endregion

    #region Message Receivers

    async void IRecipient<AddedGamesMessage>.Receive(AddedGamesMessage message) => await Task.Run(RefreshAsync);
    async void IRecipient<RemovedGamesMessage>.Receive(RemovedGamesMessage message) => await Task.Run(RefreshAsync);
    async void IRecipient<BackupLocationChangedMessage>.Receive(BackupLocationChangedMessage message) => await Task.Run(RefreshAsync);
    async void IRecipient<ModifiedGamesMessage>.Receive(ModifiedGamesMessage message)
    {
        using (await AsyncLock.LockAsync())
        {
            foreach (GameViewModel gameProgression in Games.
                         Where(x => message.GameInstallations.Contains(x.GameInstallation)))
            {
                // Only fully refresh if requested to do so when components have been rebuilt
                if (message.RebuiltComponents && gameProgression.ProgressionManager.RefreshOnRebuiltComponents)
                {
                    await gameProgression.LoadProgressAsync();
                    await gameProgression.LoadBackupAsync();
                    await gameProgression.LoadSlotInfoItemsAsync();
                }

                gameProgression.RefreshGameInfo();
            }
        }
    }
    void IRecipient<SortedGamesMessage>.Receive(SortedGamesMessage message)
    {
        Games.ModifyCollection(x =>
            x.Sort((x1, x2) => message.SortedCollection.
                IndexOf(x1.GameInstallation).
                CompareTo(message.SortedCollection.
                    IndexOf(x2.GameInstallation))));

        // Need to refresh the view if the games are
        // grouped or else the order won't update
        if (GroupGames)
            GamesView.Refresh();
    }
    async void IRecipient<ModifiedGameProgressionMessage>.Receive(ModifiedGameProgressionMessage message)
    {
        using (await AsyncLock.LockAsync())
        {
            foreach (GameViewModel game in Games)
            {
                // Check if it's the same game or if it shares the same file path
                if (game.GameInstallation == message.GameInstallation ||
                    game.Slots.Any(x => x.FilePath == message.FilePath))
                {
                    // Reload data
                    await game.LoadProgressAsync();
                    await game.LoadSlotInfoItemsAsync();
                    await game.LoadBackupAsync();
                }
            }
        }
    }

    #endregion
}