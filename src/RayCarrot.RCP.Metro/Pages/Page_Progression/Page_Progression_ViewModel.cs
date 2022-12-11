using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class Page_Progression_ViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<BackupLocationChangedMessage>
{
    #region Constructor

    public Page_Progression_ViewModel(
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
        GameItems = new ObservableCollection<GameProgressionViewModel>();
        AsyncLock = new AsyncLock();

        // Create commands
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        BackupAllCommand = new AsyncRelayCommand(BackupAllAsync);
        ChangeSaveEditProgramCommand = new AsyncRelayCommand(ChangeSaveEditProgramAsync);

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(GameItems, Application.Current);
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
    public ObservableCollection<GameProgressionViewModel> GameItems { get; }

    #endregion

    #region Protected Methods

    protected override Task InitializeAsync()
    {
        Messenger.RegisterAll(this);

        return RefreshAsync();
    }

    #endregion

    #region Public Methods

    public async void Receive(AddedGamesMessage message) => await Task.Run(async () => await RefreshAsync());
    public async void Receive(RemovedGamesMessage message) => await Task.Run(async () => await RefreshAsync());
    public async void Receive(BackupLocationChangedMessage message) => await Task.Run(async () => await RefreshAsync());

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                Logger.Info("Refreshing progression game items");

                // Clear current items
                GameItems.Clear();

                // Add the game items
                foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
                {
                    foreach (GameProgressionManager progressionManager in gameInstallation.GameDescriptor.
                                 GetComponents<ProgressionManagersComponent>().
                                 CreateManyObjects(gameInstallation))
                    {
                        GameItems.Add(new GameProgressionViewModel(progressionManager));
                    }
                }

                // TODO: Use Task.WhenAll and run in parallel?

                // Load the game items
                foreach (GameProgressionViewModel game in GameItems)
                    await game.LoadProgressAsync();

                // Load backups
                foreach (GameProgressionViewModel game in GameItems)
                    await game.LoadBackupAsync();

                // Load slot infos
                foreach (GameProgressionViewModel game in GameItems)
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
        if (GameItems.Any(x => x.IsPerformingBackupRestore))
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

            // Perform each backup
            foreach (GameProgressionViewModel game in GameItems)
            {
                if (await game.BackupAsync(true))
                    completed++;
            }

            if (completed == GameItems.Count)
                await MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Backup_BackupAllSuccess);
            else
                await MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_BackupAllFailed, completed, GameItems.Count), Resources.Backup_BackupAllFailedHeader, MessageType.Information);
        }
    }

    public async Task ChangeSaveEditProgramAsync()
    {
        ProgramSelectionResult programResult = await UI.GetProgramAsync(new ProgramSelectionViewModel()
        {
            Title = Resources.Progression_SelectEditProgram,
            ProgramFilePath = Data.Progression_SaveEditorExe,
            FileExtensions = new FileExtension[]
            {
                new FileExtension(".json"),
                new FileExtension(".txt"),
            }
        });

        if (programResult.CanceledByUser)
            return;

        Data.Progression_SaveEditorExe = programResult.ProgramFilePath;
    }

    #endregion
}