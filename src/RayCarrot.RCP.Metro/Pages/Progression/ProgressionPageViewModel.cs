﻿using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Pages.Progression;

public class ProgressionPageViewModel : BasePageViewModel, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<BackupLocationChangedMessage>, IRecipient<ModifiedGamesMessage>
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
    public ObservableCollection<GameGroupViewModel>? GameGroups { get; set; }

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
    public async void Receive(ModifiedGamesMessage message)
    {
        if (GameGroups == null)
            return;

        // As of right now there's no need to do a full refresh when a game
        // is modified, but we might have to change this in the future
        using (await AsyncLock.LockAsync())
        {
            foreach (GameViewModel gameProgression in GameGroups.
                         SelectMany(x => x.Games).
                         Where(x => message.GameInstallations.Contains(x.GameInstallation)))
            {
                gameProgression.RefreshGameInfo();
            }
        }
    }

    public async Task RefreshAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            try
            {
                Logger.Info("Refreshing progression game items");

                List<GameGroupViewModel> gameGroups = new();

                // Add the game items
                foreach (var gameInstallations in GamesManager.GetInstalledGames().GroupBy(x => x.GameDescriptor.Game))
                {
                    // Get the game info
                    GameInfoAttribute gameInfo = gameInstallations.Key.GetInfo();

                    gameGroups.Add(new GameGroupViewModel(
                        icon: gameInfo.GameIcon,
                        displayName: gameInfo.DisplayName,
                        games: gameInstallations.
                            SelectMany(x => x.GetComponents<ProgressionManagersComponent>().CreateManyObjects()).
                            Select(x => new GameViewModel(x))));
                }

                // Group games based on the backup id. Games which share the same id also share the same backup and
                // the ui should specify this to avoid confusion.
                Dictionary<string, List<GameViewModel>> backupsById = gameGroups.
                    SelectMany(x => x.Games).
                    GroupBy(x => x.ProgressionManager.BackupId).
                    ToDictionary(x => x.Key, x => x.ToList());

                // Set linked games
                foreach (GameViewModel game in gameGroups.SelectMany(x => x.Games))
                {
                    List<GameViewModel> games = backupsById[game.ProgressionManager.BackupId];
                    game.LinkedGames = new ObservableCollection<GameViewModel>(games.Where(x => x != game));
                }

                GameGroups = new ObservableCollection<GameGroupViewModel>(gameGroups);

                // TODO: Use Task.WhenAll and run in parallel?

                // Load the game items
                foreach (GameViewModel game in GameGroups.SelectMany(x => x.Games))
                    await game.LoadProgressAsync();

                // Load backups
                foreach (GameViewModel game in GameGroups.SelectMany(x => x.Games))
                    await game.LoadBackupAsync();

                // Load slot infos
                foreach (GameViewModel game in GameGroups.SelectMany(x => x.Games))
                    await game.LoadSlotInfoItemsAsync();

                Logger.Info("Refreshed progression game items");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing progression");
                GameGroups = null;
                throw;
            }
        }
    }

    public async Task BackupAllAsync()
    {
        // Make sure no backups are running
        if (GameGroups == null || GameGroups.SelectMany(x => x.Games).Any(x => x.IsPerformingBackupRestore))
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
            foreach (GameViewModel game in GameGroups.SelectMany(x => x.Games))
            {
                if (ids.Contains(game.ProgressionManager.BackupId))
                    continue;

                ids.Add(game.ProgressionManager.BackupId);

                if (await game.BackupAsync(true))
                    completed++;
            }

            int gameItemsCount = GameGroups.Sum(x => x.Games.Count);

            if (completed == gameItemsCount)
                await MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Backup_BackupAllSuccess);
            else
                await MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_BackupAllFailed, completed, gameItemsCount), Resources.Backup_BackupAllFailedHeader, MessageType.Information);
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