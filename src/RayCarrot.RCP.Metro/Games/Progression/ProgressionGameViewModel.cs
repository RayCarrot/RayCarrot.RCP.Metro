using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro;

public abstract class ProgressionGameViewModel : BaseRCPViewModel
{
    #region Constructor

    protected ProgressionGameViewModel(Games game, string? displayName = null)
    {
        // Get the info
        GameInfo gameInfo = game.GetGameInfo();

        Game = game;
        IconSource = gameInfo.IconSource;
        IsDemo = gameInfo.IsDemo;
        DisplayName = displayName ?? gameInfo.DisplayName;
        InstallDir = game.GetInstallDir(false);

        BackupInfoItems = new ObservableCollection<DuoGridItemViewModel>();
        AsyncLock = new AsyncLock();
        Slots = new ObservableCollection<ProgressionSlotViewModel>();
        BackupSlots = new ObservableCollection<ProgressionSlotViewModel>();

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

    #region Properties

    protected FileSystemPath InstallDir { get; }
    protected AsyncLock AsyncLock { get; }

    public Games Game { get; }
    public string IconSource { get; }
    public bool IsDemo { get; }
    public string DisplayName { get; }
    public bool IsLoading { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsBackupViewExpanded { get; set; }

    protected virtual string BackupName => Game.GetGameInfo().BackupName;
    protected abstract GameBackups_Directory[] BackupDirectories { get; }
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

    public ObservableCollection<ProgressionSlotViewModel> Slots { get; }
    public ObservableCollection<ProgressionSlotViewModel> BackupSlots { get; }
    public bool HasSlots { get; set; }
    public bool HasBackupSlots { get; set; }
    public ProgressionSlotViewModel? PrimarySlot { get; private set; }

    #endregion

    #region Private Methods

    private void CheckForGOGCloudSync()
    {
        // If the type is DOSBox, check if GOG cloud sync is being used
        if (Game.GetGameType() == GameType.DosBox)
        {
            try
            {
                FileSystemPath cloudSyncDir = Game.GetInstallDir(false).Parent + "cloud_saves";
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

            using BackupFileSystemWrapper backupFileSystemWrapper = new(BackupInfo!);

            await backupFileSystemWrapper.InitAsync();

            await foreach (ProgressionSlotViewModel slot in LoadSlotsAsync(backupFileSystemWrapper))
            {
                BackupSlots.Add(slot);

                // Don't allow importing or opening the save location for backup slots. Exporting is still allowed.
                slot.CanImport = false;
                slot.CanOpenLocation = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load backup progression for {0} ({1})", Game, DisplayName);

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

    #region Protected Methods

    protected virtual IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem) => AsyncEnumerable.Empty<ProgressionSlotViewModel>();

    protected virtual ProgressionSlotViewModel? GetPrimarySlot()
    {
        // Get the slot with the highest percentage from each group
        ProgressionSlotViewModel[] slots = Slots.
            GroupBy(x => x.SlotGroup).
            Select(g => g.OrderBy(x => x.Percentage).LastOrDefault()).
            ToArray();

        if (!slots.Any())
            return null;

        double totalPercentage = 0;
        List<ProgressionDataViewModel> dataItems = new();

        foreach (ProgressionSlotViewModel slot in slots)
        {
            totalPercentage += slot.Percentage / slots.Length;
            dataItems.AddRange(slot.DataItems);
        }

        return new ProgressionSlotViewModel(this, null, -1, totalPercentage, dataItems);
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
                PhysicalFileSystemWrapper fileWrapper = new(ProgramDataSource);
                await fileWrapper.InitAsync();

                // Save in temporary array. We could add to the Slots collection after each one has been asynchronously loaded
                // thus having them appear as they load in the UI, however this causes the UI to flash when refreshing an
                // already expanded game since the slots will all be removed and then re-added immediately after
                ProgressionSlotViewModel[] slots = await LoadSlotsAsync(fileWrapper).ToArrayAsync();

                Slots.Clear();
                Slots.AddRange(slots);

                PrimarySlot = GetPrimarySlot();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load progression for {0} ({1})", Game, DisplayName);

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
            Logger.Trace($"Loading backup for {Game}");

            // Set the status to syncing while the data is being loaded
            CurrentBackupStatus = BackupStatus.Syncing;

            // Create backup info if null
            BackupInfo ??= new GameBackups_BackupInfo(BackupName, BackupDirectories, DisplayName);

            // Refresh backup info
            await Task.Run(async () => await BackupInfo.RefreshAsync(ProgramDataSource));

            // Determine if the program data source can be modified
            CanChangeProgramDataSource = BackupInfo.HasVirtualStoreVersion || Data.Backup_GameDataSources.ContainsKey(BackupName);

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
        foreach (ProgressionSlotViewModel slot in Slots)
            await slot.RefreshInfoItemsAsync(Game);
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

                Logger.Trace($"Performing backup on {Game}");

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

                Logger.Trace($"Performing restore on {Game}");

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

    #region Classes

    protected abstract class FileSystemWrapper
    {
        public virtual Task InitAsync() => Task.CompletedTask;

        public abstract FileSystemPath GetFile(FileSystemPath filePath);
        public abstract IOSearchPattern? GetDirectory(IOSearchPattern searchPattern);
    }

    protected class PhysicalFileSystemWrapper : FileSystemWrapper
    {
        public PhysicalFileSystemWrapper(ProgramDataSource dataSource)
        {
            DataSource = dataSource;
        }

        public ProgramDataSource DataSource { get; }

        public override FileSystemPath GetFile(FileSystemPath filePath)
        {
            string fileName = filePath.Name;
            ProgressionDirectory dir = new(filePath.Parent, SearchOption.TopDirectoryOnly, fileName);
            return Path.Combine(dir.GetReadSearchPattern(DataSource).DirPath, fileName);
        }

        public override IOSearchPattern? GetDirectory(IOSearchPattern searchPattern)
        {
            // Convert the search pattern using the current data source
            searchPattern = new ProgressionDirectory(searchPattern).
                GetReadSearchPattern(DataSource);

            return searchPattern.DirPath.DirectoryExists 
                ? searchPattern 
                : null;
        }
    }

    protected class BackupFileSystemWrapper : FileSystemWrapper, IDisposable
    {
        public BackupFileSystemWrapper(GameBackups_BackupInfo backupInfo)
        {
            Backup = backupInfo.GetPrimaryBackup;
            RestoreDirectories = backupInfo.RestoreDirectories ?? throw new Exception($"Restore directories must be set");

            if (Backup == null)
                return;

            if (Backup.IsCompressed)
            {
                TempExtractDir = new TempDirectory(true);
                BackupDir = TempExtractDir.TempPath;
            }
            else
            {
                BackupDir = Backup.Path;
            }
        }

        public GameBackups_ExistingBackup? Backup { get; }
        public BackupSearchPattern[] RestoreDirectories { get; }
        public FileSystemPath BackupDir { get; }
        public TempDirectory? TempExtractDir { get; }

        public override async Task InitAsync()
        {
            if (Backup is null || TempExtractDir is null)
                return;

            await Task.Run(() =>
            {
                using ZipArchive zip = new(File.OpenRead(Backup.Path));
                zip.ExtractToDirectory(TempExtractDir.TempPath);
            });
        }

        public override FileSystemPath GetFile(FileSystemPath filePath)
        {
            if (Backup == null)
                return FileSystemPath.EmptyPath;

            foreach (BackupSearchPattern dir in RestoreDirectories)
            {
                if (!filePath.FullPath.StartsWith(dir.SearchPattern.DirPath))
                    continue;
                
                FileSystemPath backupPath = BackupDir + dir.ID + (filePath - dir.SearchPattern.DirPath);

                if (backupPath.FileExists)
                    return backupPath;
            }

            return FileSystemPath.EmptyPath;
        }

        public override IOSearchPattern? GetDirectory(IOSearchPattern searchPattern)
        {
            if (Backup == null)
                return null;

            FileSystemPath backupDir = FileSystemPath.EmptyPath;

            foreach (BackupSearchPattern dir in RestoreDirectories)
            {
                if (!searchPattern.DirPath.ContainsPath(dir.SearchPattern.DirPath))
                    continue;

                FileSystemPath backupPath = BackupDir + dir.ID + (searchPattern.DirPath - dir.SearchPattern.DirPath);

                if (backupPath.DirectoryExists)
                {
                    backupDir = backupPath;
                    break;
                }
            }

            if (!backupDir.DirectoryExists)
                return null;

            return new IOSearchPattern(backupDir, searchPattern.SearchOption, searchPattern.SearchPattern);
        }

        public void Dispose()
        {
            TempExtractDir?.Dispose();
        }
    }

    #endregion
}