using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Pages.Progression;

public class ProgressionPageViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>, 
    IRecipient<SortedGamesMessage>, IRecipient<BackupLocationChangedMessage>
{
    #region Constructor

    public ProgressionPageViewModel(
        AppViewModel app, 
        AppUserData data, 
        IMessageUIManager messageUi, 
        AppUIManager ui, 
        GamesManager gamesManager, 
        IMessenger messenger) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        // Create properties
        AsyncLock = new AsyncLock();
        Games = new ObservableCollectionEx<GameViewModel>();
        GamesView = CollectionViewSource.GetDefaultView(Games);

        RefreshGrouping(GroupGames);

        // Create commands
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        BackupAllCommand = new AsyncRelayCommand(BackupAllAsync);
        ChangeSaveEditProgramCommand = new AsyncRelayCommand(ChangeSaveEditProgramAsync);
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
    public ICommand BackupAllCommand { get; }
    public ICommand ChangeSaveEditProgramCommand { get; }

    #endregion

    #region Services

    public AppUserData Data { get; } // Need to keep public for now due to binding
    private IMessageUIManager MessageUI { get; }
    private AppUIManager UI { get; }
    private GamesManager GamesManager { get; }
    private IMessenger Messenger { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Progression;
    public ObservableCollectionEx<GameViewModel> Games { get; }
    public ICollectionView GamesView { get; }

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

    #endregion

    #region Protected Methods

    protected override Task InitializeAsync()
    {
        Messenger.RegisterAll(this);

        return RefreshAsync();
    }

    #endregion

    #region Public Methods

    public async void Receive(AddedGamesMessage message) => await Task.Run(RefreshAsync);
    public async void Receive(RemovedGamesMessage message) => await Task.Run(RefreshAsync);
    public async void Receive(BackupLocationChangedMessage message) => await Task.Run(RefreshAsync);
    public async void Receive(ModifiedGamesMessage message)
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
    public void Receive(SortedGamesMessage message)
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

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                Logger.Info("Refreshing progression game items");

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

    public async Task BackupAllAsync()
    {
        // Make sure no backups are running
        if (Games.Any(x => x.IsPerformingBackupRestore))
            return;

        // Lock
        using (await AsyncLock.LockAsync())
        {
            // TODO-UPDATE: Update string to also say "If multiple games share the same backup then a backup will only be performed for the first game".
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

    public async Task ChangeSaveEditProgramAsync()
    {
        ProgramSelectionResult programResult = await UI.GetProgramAsync(new ProgramSelectionViewModel()
        {
            Title = Resources.Progression_SelectEditProgram,
            ProgramFilePath = Data.Progression_SaveEditorExe,
            FileExtensions = new FileExtension[] { new(".json"), new(".txt"), }
        });

        if (programResult.CanceledByUser)
            return;

        Data.Progression_SaveEditorExe = programResult.ProgramFilePath;
    }

    #endregion
}