using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Pages.Progression;

// TODO: Handle or show warning if cloud sync is detected for the game, such as through GOG Galaxy (which redirects files)
public class GameViewModel : BaseRCPViewModel
{
    #region Constructor

    public GameViewModel(GameProgressionManager progressionManager, GameGroupViewModel gameGroup)
    {
        ProgressionManager = progressionManager;
        GameGroup = gameGroup;
        ProgressionName = progressionManager.Name;
        BackupInfoItems = new ObservableCollection<DuoGridItemViewModel>();
        AsyncLock = new AsyncLock();
        Slots = new ObservableCollection<GameSlotViewModel>();
        BackupSlots = new ObservableCollection<GameSlotViewModel>();

        // Get and set platform info
        GamePlatformInfoAttribute platformInfo = GameDescriptor.Platform.GetInfo();
        PlatformDisplayName = platformInfo.DisplayName;
        PlatformIcon = platformInfo.Icon;

        RefreshGameInfo();

        UpdateProgramDataSourceCommand = new AsyncRelayCommand(UpdateProgramDataSourceAsync);
        LoadBackupViewCommand = new AsyncRelayCommand(LoadBackupViewAsync);
        BackupCommand = new AsyncRelayCommand(async () => await BackupAsync());
        RestoreCommand = new AsyncRelayCommand(RestoreAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand UpdateProgramDataSourceCommand { get; }
    public ICommand LoadBackupViewCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand RestoreCommand { get; }

    #endregion

    #region Private Fields

    private bool _hasLoadedBackupView;

    #endregion

    #region Private Properties

    private AsyncLock AsyncLock { get; }

    #endregion

    #region Public Properties

    public GameProgressionManager ProgressionManager { get; }
    public GameGroupViewModel GameGroup { get; }
    public ObservableCollection<GameViewModel>? LinkedGames { get; set; } // Games which share the same backup id
    public GameInstallation GameInstallation => ProgressionManager.GameInstallation;
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;
    public GamePlatformIconAsset PlatformIcon { get; }
    public LocalizedString PlatformDisplayName { get; }
    public LocalizedString DisplayName { get; set; }
    public string? ProgressionName { get; }
    public bool IsLoading { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsBackupViewExpanded { get; set; }

    public BackupStatus CurrentBackupStatus { get; set; }
    public GameBackups_BackupInfo? BackupInfo { get; set; }
    public ObservableCollection<DuoGridItemViewModel> BackupInfoItems { get; }
    public bool HasBackupInfoItems { get; set; }
    public bool IsPerformingBackupRestore { get; set; }
    public bool ShowBackupRestoreIndicator { get; set; }
    public bool CanPerformBackup { get; set; }
    public bool CanRestoreBackup { get; set; }
    public bool CanChangeProgramDataSource { get; set; }
    public bool HasBackup { get; set; }

    public ProgramDataSource ProgramDataSource
    {
        get
        {
            var dataSources = GameInstallation.GetObject<ProgressionDataSources>(GameDataKey.Progression_DataSources);
            return dataSources?.DataSources.TryGetValue(ProgressionManager.ProgressionId, out ProgramDataSource src) == true
                ? src
                : ProgramDataSource.Auto;
        }
        set
        {
            GameInstallation.ModifyObject<ProgressionDataSources>(GameDataKey.Progression_DataSources, 
                x => x.DataSources[ProgressionManager.ProgressionId] = value);
        }
    }

    public ObservableCollection<GameSlotViewModel> Slots { get; }
    public ObservableCollection<GameSlotViewModel> BackupSlots { get; }
    public bool HasSlots { get; set; }
    public bool HasBackupSlots { get; set; }
    public GameSlotViewModel? PrimarySlot { get; private set; }

    public bool IsGameGrouped { get; set; }

    #endregion

    #region Private Methods

    private async Task LoadBackupInfoAsync(GameBackups_ExistingBackup? backup)
    {
        BackupInfoItems.Clear();

        // NOTE: Not localized due to being debug only
        BackupInfoItems.Add(new DuoGridItemViewModel(
            header: "Latest backup version", 
            text: BackupInfo!.LatestAvailableBackupVersion.ToString(), 
            minUserLevel: UserLevel.Debug));

        HasBackupInfoItems = Services.Data.App_UserLevel == UserLevel.Debug;

        if (backup != null)
        {
            // Load backup info
            try
            {
                // NOTE: Not localized due to being debug only
                BackupInfoItems.Add(new DuoGridItemViewModel(
                    header: "Backup version", 
                    text: backup.BackupVersion.ToString(), 
                    minUserLevel: UserLevel.Debug));
                BackupInfoItems.Add(new DuoGridItemViewModel(
                    header: "Is backup compressed", 
                    text: backup.IsCompressed.ToString(), 
                    minUserLevel: UserLevel.Debug));

                DateTime lastWriteTime = backup.Path.GetFileSystemInfo().LastWriteTime;

                // Get the backup date
                BackupInfoItems.Add(new DuoGridItemViewModel(
                    header: new ResourceLocString(nameof(Resources.Backup_LastBackupDate)), 
                    text: new GeneratedLocString(() => lastWriteTime.ToShortDateString())));

                // Get the backup size
                BackupInfoItems.Add(new DuoGridItemViewModel(
                    header: new ResourceLocString(nameof(Resources.Backup_LastBackupSize)), 
                    text: backup.Path.GetSize().ToString()));

                HasBackupInfoItems = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Getting existing backup info");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.ReadingBackupError, DisplayName));
            }
        }

        // Add more debug info (not localized). Only add if user level is Debug to avoid slowing down the loading.
        if (Data.App_UserLevel >= UserLevel.Debug)
        {
            foreach (BackupSearchPattern dir in BackupInfo.BackupDirectories!)
            {
                BackupInfoItems.Add(new DuoGridItemViewModel(
                    header: $"BackupDir[{dir.ID}]", 
                    text: $"{dir.SearchPattern.DirPath} ({dir.SearchPattern.SearchPattern}, {dir.SearchPattern.SearchOption})", 
                    minUserLevel: UserLevel.Debug));
            }

            foreach (BackupSearchPattern dir in BackupInfo.RestoreDirectories!)
            {
                BackupInfoItems.Add(new DuoGridItemViewModel(
                    header: $"RestoreDir[{dir.ID}]", 
                    text: $"{dir.SearchPattern.DirPath} ({dir.SearchPattern.SearchPattern}, {dir.SearchPattern.SearchOption})", 
                    minUserLevel: UserLevel.Debug));
            }
        }
    }

    private async Task LoadBackupSlotsAsync()
    {
        // Load backup progression
        try
        {
            BackupSlots.Clear();

            using GameProgressionManager.BackupFileSystemWrapper backupFileSystemWrapper = new(BackupInfo!);

            await backupFileSystemWrapper.InitAsync();

            await foreach (GameProgressionSlot slot in ProgressionManager.LoadSlotsAsync(backupFileSystemWrapper))
            {
                // Don't allow importing or opening the save location for backup slots. Exporting is still allowed.
                GameSlotViewModel slotViewModel = new(this, slot, canOpenLocation: false)
                {
                    CanImport = false
                };

                BackupSlots.Add(slotViewModel);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load backup progression for {0} ({1})", GameInstallation.FullId, DisplayName);

            BackupSlots.Clear();
        }

        HasBackupSlots = BackupSlots.Any();
    }

    private async Task<BackupStatus> GetBackupStatusAsync(GameBackups_ExistingBackup backup)
    {
        // If the backup is not using the latest version we always mark it as being outdated
        if (backup.BackupVersion < BackupInfo!.LatestAvailableBackupVersion)
            return BackupStatus.Outdated;

        // There are several ways we could compare the current progress with the backup data. The most accurate is checking if the
        // bytes match, but that's slow. The faster way is to check the files, write dates and file sizes.

        ZipArchive? compressedBackup = null;

        try
        {
            if (backup.IsCompressed)
                compressedBackup = new ZipArchive(File.OpenRead(backup.Path));

            DateTime backupDate = backup.Path.GetFileSystemInfo().LastWriteTime;
            BackupSearchPattern[] backupDirs = BackupInfo.BackupDirectories!;

            // Get the current progress files
            var currentFiles = backupDirs.Where(x => x.SearchPattern.DirPath.DirectoryExists).Select(x => new
            {
                ID = x.ID,
                Files = x.SearchPattern.GetFiles(),
                BasePath = x.SearchPattern.DirPath,
            }).SelectMany(x => x.Files.Select(f => new
            {
                ID = x.ID,
                FilePath = (FileSystemPath)f,
                RelativeFilePath = (FileSystemPath)Path.Combine(x.ID, new FileSystemPath(f) - x.BasePath),
            }));

            int filesCount = 0;

            foreach (var f in currentFiles)
            {
                FileInfo fileInfo = f.FilePath.GetFileInfo();

                // Check write date. If it's later than the backup then we assume the backup is outdated.
                if (fileInfo.LastWriteTime > backupDate)
                    return BackupStatus.Outdated;

                if (compressedBackup != null)
                {
                    // Attempt to get the matching file in the backup
                    ZipArchiveEntry? entry = compressedBackup.GetEntry(f.RelativeFilePath);

                    // If it doesn't exist the backup is outdated
                    if (entry == null)
                        return BackupStatus.Outdated;

                    // Check the file size
                    if (entry.Length != fileInfo.Length)
                        return BackupStatus.Outdated;
                }
                else
                {
                    FileSystemPath path = backup.Path + f.RelativeFilePath;

                    // Check if the file exists
                    if (!path.FileExists)
                        return BackupStatus.Outdated;

                    // Check the file size
                    if (path.GetFileInfo().Length != fileInfo.Length)
                        return BackupStatus.Outdated;
                }

                filesCount++;
            }

            // Make sure the backup doesn't have additional files not in the current progress
            if (compressedBackup != null)
            {
                if (compressedBackup.Entries.Count != filesCount)
                    return BackupStatus.Outdated;
            }
            else
            {
                if (Directory.EnumerateFiles(backup.Path, "*", SearchOption.AllDirectories).Count() != filesCount)    
                    return BackupStatus.Outdated;
            }

            // If all checks passed we assume the backup is up to date
            return BackupStatus.UpToDate;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting backup status for {0} ({1})", GameInstallation.FullId, DisplayName);

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.ReadingBackupError, DisplayName));

            return BackupStatus.None;
        }
        finally
        {
            compressedBackup?.Dispose();
        }
    }

    #endregion

    #region Public Methods

    [MemberNotNull(nameof(DisplayName))]
    public void RefreshGameInfo() => DisplayName = GameInstallation.GetDisplayName();

    public async Task UpdateProgramDataSourceAsync()
    {
        Logger.Trace("Updating program data source for {0} ({1})", GameInstallation.FullId, DisplayName);

        await LoadProgressAsync();
        await LoadBackupAsync();
        await LoadSlotInfoItemsAsync();
    }

    public async Task LoadBackupViewAsync()
    {
        if (_hasLoadedBackupView)
            return;

        Logger.Trace("First time loading backup view for {0} ({1})", GameInstallation.FullId, DisplayName);

        await LoadBackupInfoAsync(BackupInfo?.GetPrimaryBackup);
        await LoadBackupSlotsAsync();

        _hasLoadedBackupView = true;
    }

    public async Task LoadProgressAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            IsLoading = true;

            try
            {
                GameProgressionManager.PhysicalFileSystemWrapper fileWrapper = new(ProgramDataSource);
                await fileWrapper.InitAsync();

                // Save in temporary array. We could add to the Slots collection after each one has been asynchronously loaded
                // thus having them appear as they load in the UI, however this causes the UI to flash when refreshing an
                // already expanded game since the slots will all be removed and then re-added immediately after
                GameProgressionSlot[] slots = await ProgressionManager.LoadSlotsAsync(fileWrapper).ToArrayAsync();

                Slots.Clear();
                Slots.AddRange(slots.Select(x => new GameSlotViewModel(this, x)));

                GameProgressionSlot? primarySlot = GameProgressionManager.CreatePrimarySlot(Slots.Select(x => x.Slot));
                PrimarySlot = primarySlot == null ? null : new GameSlotViewModel(this, primarySlot);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load progression for {0} ({1})", GameInstallation.FullId, DisplayName);

                Slots.Clear();
                PrimarySlot = null;
            }
            finally
            {
                IsLoading = false;
            }

            HasSlots = Slots.Any();
        }
    }

    public async Task LoadBackupAsync()
    {
        if (!ProgressionManager.SupportsBackups)
        {
            Logger.Trace($"{GameInstallation.FullId} does not support backups");

            CanPerformBackup = false;
            CanRestoreBackup = false;
            CurrentBackupStatus = BackupStatus.None;
            return;
        }

        CanPerformBackup = true;

        using (await AsyncLock.LockAsync())
        {
            Logger.Trace($"Loading backup for {GameInstallation.FullId}");

            // Set the status to syncing while the data is being loaded
            CurrentBackupStatus = BackupStatus.Syncing;

            // Create backup info if null
            BackupInfo ??= new GameBackups_BackupInfo(ProgressionManager.ProgressionId, ProgressionManager.BackupDirectories);

            // Refresh backup info
            await Task.Run(async () => await BackupInfo.RefreshAsync(ProgramDataSource, DisplayName));

            // Determine if the program data source can be modified
            CanChangeProgramDataSource = BackupInfo.HasVirtualStoreVersion || ProgramDataSource != ProgramDataSource.Auto;

            // Get the primary backup
            GameBackups_ExistingBackup? backup = BackupInfo.GetPrimaryBackup;

            // Mark that we can restore a backup if there is one
            CanRestoreBackup = backup != null;
            
            // Indicate if there is an existing backup or not
            HasBackup = backup != null;

            // Get the current backup status
            CurrentBackupStatus = backup != null ? await Task.Run(async () => await GetBackupStatusAsync(backup)) : BackupStatus.None;

            // Update the backup view if it was previously loaded
            if (_hasLoadedBackupView)
            {
                Logger.Trace("Reloading backup view for {0} ({1})", GameInstallation.FullId, DisplayName);

                await LoadBackupInfoAsync(backup);
                await LoadBackupSlotsAsync();
            }
        }
    }

    public async Task LoadSlotInfoItemsAsync()
    {
        foreach (GameSlotViewModel slot in Slots)
            await slot.RefreshInfoItemsAsync(GameInstallation);
    }

    public async Task<bool> BackupAsync(bool fromBatchOperation = false)
    {
        if (IsPerformingBackupRestore)
            return false;

        if (BackupInfo == null)
            return false;

        if (!CanPerformBackup)
            return false;

        bool success;

        try
        {
            using (await AsyncLock.LockAsync())
            {
                IsPerformingBackupRestore = true;

                Logger.Trace($"Performing backup on {GameInstallation.FullId}");

                // Refresh the backup info
                await BackupInfo.RefreshAsync(ProgramDataSource, DisplayName);

                // TODO-UPDATE: Update string to also show linked games
                // Confirm backup if one already exists
                if (!fromBatchOperation && 
                    BackupInfo.ExistingBackups.Any() && 
                    !await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Confirm, DisplayName), Resources.Backup_ConfirmHeader, MessageType.Warning, true))
                {
                    Logger.Info("Backup canceled");
                    return false;
                }

                ShowBackupRestoreIndicator = true;

                try
                {
                    // Perform the backup
                    success = await Task.Run(async () => await Services.Backup.BackupAsync(BackupInfo, DisplayName));
                }
                finally
                {
                    ShowBackupRestoreIndicator = false;
                }
            }

            await LoadBackupAsync();

            if (LinkedGames != null)
                foreach (GameViewModel game in LinkedGames)
                    await game.LoadBackupAsync();

            if (success && !fromBatchOperation)
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Backup_Success, DisplayName), Resources.Backup_SuccessHeader);
        }
        finally
        {
            IsPerformingBackupRestore = false;
        }

        return success;
    }

    public async Task RestoreAsync()
    {
        if (IsPerformingBackupRestore)
            return;

        if (BackupInfo == null)
            return;

        if (!CanRestoreBackup)
            return;

        try
        {
            bool backupResult;

            using (await AsyncLock.LockAsync())
            {
                IsPerformingBackupRestore = true;

                Logger.Trace($"Performing restore on {GameInstallation.FullId}");

                // Refresh the backup info
                await BackupInfo.RefreshAsync(ProgramDataSource, DisplayName);

                // Confirm restore
                if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Confirm, DisplayName), Resources.Restore_ConfirmHeader, MessageType.Warning, true))
                {
                    Logger.Info("Restore canceled");

                    return;
                }

                ShowBackupRestoreIndicator = true;

                try
                {
                    // Perform the restore
                    backupResult = await Task.Run(async () => await Services.Backup.RestoreAsync(BackupInfo, DisplayName));
                }
                finally
                {
                    ShowBackupRestoreIndicator = false;
                }
            }

            await LoadProgressAsync();
            await LoadSlotInfoItemsAsync();
            await LoadBackupAsync();

            if (backupResult)
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Restore_Success, DisplayName), Resources.Restore_SuccessHeader);
        }
        finally
        {
            IsPerformingBackupRestore = false;
        }
    }

    #endregion

    #region Enums

    public enum BackupStatus
    {
        None,
        UpToDate,
        Outdated,
        Syncing,
    }

    #endregion
}