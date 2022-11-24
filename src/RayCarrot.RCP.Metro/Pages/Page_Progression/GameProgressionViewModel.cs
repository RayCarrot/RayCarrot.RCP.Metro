﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

// TODO-14: The progression system has to be updated to work with multiple game installations. For simplicity they should share the
//          same backup if the game itself is identical (i.e. Steam and Win32 are the same release, but PS1 is not!).

// TODO-14: Rename to GameProgressionViewModel
public class GameProgressionViewModel : BaseRCPViewModel
{
    #region Constructor

    public GameProgressionViewModel(GameProgressionManager progressionManager)
    {
        ProgressionManager = progressionManager;
        BackupInfoItems = new ObservableCollection<DuoGridItemViewModel>();
        AsyncLock = new AsyncLock();
        Slots = new ObservableCollection<GameProgressionSlotViewModel>();
        BackupSlots = new ObservableCollection<GameProgressionSlotViewModel>();

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
    private string BackupName => GameDescriptor.BackupName;

    #endregion

    #region Public Properties

    public GameProgressionManager ProgressionManager { get; }
    public GameInstallation GameInstallation => ProgressionManager.GameInstallation;
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;
    public string IconSource => GameDescriptor.IconSource;
    public bool IsDemo => GameDescriptor.IsDemo;
    public string DisplayName => GameDescriptor.DisplayName; // TODO: LocalizedString
    public bool IsLoading { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsBackupViewExpanded { get; set; }

    public BackupStatus CurrentBackupStatus { get; set; }
    public bool IsGOGCloudSyncUsed { get; set; }
    public GameBackups_BackupInfo? BackupInfo { get; set; }
    public ObservableCollection<DuoGridItemViewModel> BackupInfoItems { get; }
    public bool HasBackupInfoItems { get; set; }
    public bool IsPerformingBackupRestore { get; set; }
    public bool ShowBackupRestoreIndicator { get; set; }
    public bool CanRestoreBackup { get; set; }
    public bool CanChangeProgramDataSource { get; set; }
    public bool HasBackup { get; set; }

    public ProgramDataSource ProgramDataSource
    {
        get => Data.Backup_GameDataSources.TryGetValue(BackupName, ProgramDataSource.Auto);
        set => Data.Backup_GameDataSources[BackupName] = value;
    }

    public ObservableCollection<GameProgressionSlotViewModel> Slots { get; }
    public ObservableCollection<GameProgressionSlotViewModel> BackupSlots { get; }
    public bool HasSlots { get; set; }
    public bool HasBackupSlots { get; set; }
    public GameProgressionSlotViewModel? PrimarySlot { get; private set; }

    #endregion

    #region Private Methods

    private void CheckForGOGCloudSync()
    {
        // TODO-14: This should not be handled here and definitely not by checking if the platform is MSDOS
        // If the type is DOSBox, check if GOG cloud sync is being used
        if (GameDescriptor.Platform == GamePlatform.MSDOS)
        {
            try
            {
                FileSystemPath cloudSyncDir = GameInstallation.InstallLocation.Parent + "cloud_saves";
                IsGOGCloudSyncUsed = cloudSyncDir.DirectoryExists && Directory.EnumerateFileSystemEntries(cloudSyncDir).Any();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Getting if DOSBox game is using GOG cloud sync");
                IsGOGCloudSyncUsed = false;
            }
        }
        else
        {
            IsGOGCloudSyncUsed = false;
        }
    }

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
                GameProgressionSlotViewModel slotViewModel = new(this, slot, canOpenLocation: false)
                {
                    CanImport = false
                };

                BackupSlots.Add(slotViewModel);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load backup progression for {0} ({1})", GameInstallation.Id, DisplayName);

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
            Logger.Error(ex, "Getting backup status for {0}", DisplayName);

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

    public async Task UpdateProgramDataSourceAsync()
    {
        Logger.Trace($"Updating program data source for {DisplayName}");

        await LoadProgressAsync();
        await LoadBackupAsync();
        await LoadSlotInfoItemsAsync();
    }

    public async Task LoadBackupViewAsync()
    {
        if (_hasLoadedBackupView)
            return;

        Logger.Trace($"First time loading backup view for {DisplayName}");

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
                Slots.AddRange(slots.Select(x => new GameProgressionSlotViewModel(this, x)));

                GameProgressionSlot? primarySlot = GameProgressionManager.CreatePrimarySlot(Slots.Select(x => x.Slot));
                PrimarySlot = primarySlot == null ? null : new GameProgressionSlotViewModel(this, primarySlot);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load progression for {0} ({1})", GameInstallation.Id, DisplayName);

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
        using (await AsyncLock.LockAsync())
        {
            Logger.Trace($"Loading backup for {GameInstallation.Id}");

            // Set the status to syncing while the data is being loaded
            CurrentBackupStatus = BackupStatus.Syncing;

            // Create backup info if null
            BackupInfo ??= new GameBackups_BackupInfo(BackupName, ProgressionManager.BackupDirectories, DisplayName);

            // Refresh backup info
            await Task.Run(async () => await BackupInfo.RefreshAsync(ProgramDataSource));

            // Determine if the program data source can be modified
            CanChangeProgramDataSource = BackupInfo.HasVirtualStoreVersion || 
                                         (Data.Backup_GameDataSources.TryGetValue(BackupName, out ProgramDataSource src) && src != ProgramDataSource.Auto);

            // Check if GOG cloud sync is in use
            CheckForGOGCloudSync();

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
                Logger.Trace($"Reloading backup view for {DisplayName}");

                await LoadBackupInfoAsync(backup);
                await LoadBackupSlotsAsync();
            }
        }
    }

    public async Task LoadSlotInfoItemsAsync()
    {
        foreach (GameProgressionSlotViewModel slot in Slots)
            await slot.RefreshInfoItemsAsync(GameInstallation);
    }

    public async Task<bool> BackupAsync(bool fromBatchOperation = false)
    {
        if (IsPerformingBackupRestore)
            return false;

        if (BackupInfo == null)
            return false;

        bool success;

        try
        {
            using (await AsyncLock.LockAsync())
            {
                IsPerformingBackupRestore = true;

                Logger.Trace($"Performing backup on {GameInstallation.Id}");

                // Show a warning message if GOG cloud sync is being used for this game as that will redirect the game data to its own directory
                if (IsGOGCloudSyncUsed && !fromBatchOperation)
                    await Services.MessageUI.DisplayMessageAsync(Resources.Backup_GOGSyncWarning, Resources.Backup_GOGSyncWarningHeader, MessageType.Warning);

                // Refresh the backup info
                await BackupInfo.RefreshAsync(ProgramDataSource);

                // Confirm backup if one already exists
                if (!fromBatchOperation && 
                    BackupInfo.ExistingBackups.Any() && 
                    !await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Confirm, BackupInfo.GameDisplayName), Resources.Backup_ConfirmHeader, MessageType.Warning, true))
                {
                    Logger.Info("Backup canceled");
                    return false;
                }

                ShowBackupRestoreIndicator = true;

                try
                {
                    // Perform the backup
                    success = await Task.Run(async () => await Services.Backup.BackupAsync(BackupInfo));
                }
                finally
                {
                    ShowBackupRestoreIndicator = false;
                }
            }

            await LoadBackupAsync();

            if (success && !fromBatchOperation)
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Backup_Success, BackupInfo.GameDisplayName), Resources.Backup_SuccessHeader);
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

                Logger.Trace($"Performing restore on {GameInstallation.Id}");

                // Show a warning message if GOG cloud sync is being used for this game as that will redirect the game data to its own directory
                if (IsGOGCloudSyncUsed)
                    await Services.MessageUI.DisplayMessageAsync(Resources.Backup_GOGSyncWarning, Resources.Backup_GOGSyncWarningHeader, MessageType.Warning);

                // Refresh the backup info
                await BackupInfo.RefreshAsync(ProgramDataSource);

                // Confirm restore
                if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Confirm, BackupInfo.GameDisplayName), Resources.Restore_ConfirmHeader, MessageType.Warning, true))
                {
                    Logger.Info("Restore canceled");

                    return;
                }

                ShowBackupRestoreIndicator = true;

                try
                {
                    // Perform the restore
                    backupResult = await Task.Run(async () => await Services.Backup.RestoreAsync(BackupInfo));
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
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Restore_Success, BackupInfo.GameDisplayName), Resources.Restore_SuccessHeader);
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