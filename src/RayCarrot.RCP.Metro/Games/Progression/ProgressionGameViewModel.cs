using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

public abstract class ProgressionGameViewModel : BaseViewModel
{
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

        BackupCommand = new AsyncRelayCommand(async () => await BackupAsync());
        RestoreCommand = new AsyncRelayCommand(RestoreAsync);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ICommand BackupCommand { get; }
    public ICommand RestoreCommand { get; }

    protected FileSystemPath InstallDir { get; }
    protected AsyncLock AsyncLock { get; }

    public Games Game { get; }
    public string IconSource { get; }
    public bool IsDemo { get; }
    public string DisplayName { get; }
    public bool IsLoading { get; set; }
    public bool IsExpanded { get; set; }

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

    public ObservableCollection<ProgressionSlotViewModel> Slots { get; }
    public ObservableCollection<ProgressionSlotViewModel> BackupSlots { get; }
    public ProgressionSlotViewModel? PrimarySlot { get; private set; }

    protected Task<T?> SerializeFileDataAsync<T>(FileSystemWrapper fileSystem, FileSystemPath filePath, BinarySerializerSettings settings, IDataEncoder? encoder = null)
        where T : class, IBinarySerializable, new()
    {
        return Task.Run(() =>
        {
            Stream? fileStream = null;
            Stream? decodedStream = null;

            try
            {
                fileStream = fileSystem.ReadFile(filePath);

                if (fileStream == null)
                    return null;

                if (encoder != null)
                {
                    // Create a memory stream
                    decodedStream = new MemoryStream();

                    // Decode the data
                    encoder.Decode(fileStream, decodedStream);

                    // Set the position
                    decodedStream.Position = 0;
                }

                return BinarySerializableHelpers.ReadFromStream<T>(decodedStream ?? fileStream, settings, Services.App.GetBinarySerializerLogger(filePath.Name));
            }
            finally
            {
                fileStream?.Dispose();
                decodedStream?.Dispose();
            }
        });
    }

    protected virtual IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem) => AsyncEnumerable.Empty<ProgressionSlotViewModel>();

    protected virtual ProgressionSlotViewModel? GetPrimarySlot()
    {
        // Get the slot with the highest percentage from each group
        ProgressionSlotViewModel[] slots = Slots.
            GroupBy(x => x.SlotGroup).
            Select(g => g.OrderBy(x => x.Percentage).LastOrDefault()).
            ToArray();

        double totalPercentage = 0;
        List<ProgressionDataViewModel> dataItems = new();

        foreach (ProgressionSlotViewModel slot in slots)
        {
            totalPercentage += slot.Percentage / slots.Length;
            dataItems.AddRange(slot.DataItems);
        }

        return new ProgressionSlotViewModel(this, null, -1, totalPercentage, dataItems);
    }

    public async Task LoadProgressAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            IsLoading = true;

            try
            {
                // Save in temporary array. We could add to the Slots collection after each one has been asynchronously loaded
                // thus having them appear as they load in the UI, however this causes the UI to flash when refreshing an
                // already expanded game since the slots will all be removed and then re-added immediately after
                ProgressionSlotViewModel[] slots = await LoadSlotsAsync(new PhysicalFileSystemWrapper()).ToArrayAsync();

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
        }
    }

    public async Task LoadBackupAsync()
    {
        using (await AsyncLock.LockAsync())
        {
            Logger.Trace($"Loading backup for {Game}");

            CurrentBackupStatus = BackupStatus.Syncing;

            // Create backup info if null
            BackupInfo ??= new GameBackups_BackupInfo(BackupName, BackupDirectories, DisplayName);

            // Run as a task
            await Task.Run(async () =>
            {
                // Refresh backup info
                await BackupInfo.RefreshAsync();

                // If the type is DOSBox, check if GOG cloud sync is being used
                if (Game.GetGameType() == GameType.DosBox)
                {
                    try
                    {
                        FileSystemPath cloudSyncDir = Game.GetInstallDir(false).Parent + "cloud_saves";
                        IsGOGCloudSyncUsed = cloudSyncDir.DirectoryExists && Directory.GetFileSystemEntries(cloudSyncDir).Any();
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
            });

            // Get the primary backup
            GameBackups_ExistingBackup? backup = BackupInfo.GetPrimaryBackup;

            BackupInfoItems.Clear();

            // NOTE: Not localized due to being debug only
            BackupInfoItems.Add(new DuoGridItemViewModel("Latest backup version", BackupInfo.LatestAvailableBackupVersion.ToString(), UserLevel.Debug));

            HasBackupInfoItems = Services.Data.App_UserLevel == UserLevel.Debug;

            if (backup != null)
            {
                // Load backup info
                try
                {
                    // NOTE: Not localized due to being debug only
                    BackupInfoItems.Add(new DuoGridItemViewModel("Backup version", backup.BackupVersion.ToString(), UserLevel.Debug));
                    BackupInfoItems.Add(new DuoGridItemViewModel("Is backup compressed", backup.IsCompressed.ToString(), UserLevel.Debug));

                    // Get the backup date
                    // TODO-UPDATE: Update localized character casing
                    BackupInfoItems.Add(new DuoGridItemViewModel(Resources.Backup_LastBackupDate, backup.Path.GetFileSystemInfo().LastWriteTime.ToShortDateString()));

                    // Get the backup size
                    // TODO-UPDATE: Update localized character casing
                    BackupInfoItems.Add(new DuoGridItemViewModel(Resources.Backup_LastBackupSize, backup.Path.GetSize().ToString()));

                    // TODO-UPDATE: Implement - compare files to backup to check for differences
                    CurrentBackupStatus = BackupStatus.UpToDate;

                    HasBackupInfoItems = true;
                    CanRestoreBackup = true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Getting existing backup info");

                    CanRestoreBackup = false;
                    CurrentBackupStatus = BackupStatus.Error;

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.ReadingBackupError, DisplayName));
                }

                // Load backup progression
                try
                {
                    BackupSlots.Clear();

                    await foreach (ProgressionSlotViewModel slot in LoadSlotsAsync(new BackupFileSystemWrapper()))
                        BackupSlots.Add(slot);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to load backup progression for {0} ({1})", Game, DisplayName);

                    BackupSlots.Clear();
                }
            }
            else
            {
                CanRestoreBackup = false;
                CurrentBackupStatus = BackupStatus.None;
            }
        }
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
                await BackupInfo.RefreshAsync();

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
                await BackupInfo.RefreshAsync();

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

            if (backupResult)
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Restore_Success, BackupInfo.GameDisplayName), Resources.Restore_SuccessHeader);
        }
        finally
        {
            IsPerformingBackupRestore = false;
            await LoadBackupAsync();
        }
    }

    public enum BackupStatus
    {
        None,
        UpToDate,
        Outdated,
        Syncing,
        Error,
    }

    protected abstract class FileSystemWrapper
    {
        public abstract Stream? ReadFile(string filePath);
        public abstract IEnumerable<string> GetFiles(string dirPath);
        public abstract IEnumerable<string> GetDirectories(string dirPath);
    }
    protected class PhysicalFileSystemWrapper : FileSystemWrapper
    {
        public override Stream? ReadFile(string filePath) => !File.Exists(filePath) ? null : File.OpenRead(filePath);
        public override IEnumerable<string> GetFiles(string dirPath) => !Directory.Exists(dirPath) ? Enumerable.Empty<string>() : Directory.GetFiles(dirPath);
        public override IEnumerable<string> GetDirectories(string dirPath) => !Directory.Exists(dirPath) ? Enumerable.Empty<string>() : Directory.GetDirectories(dirPath);
    }
    protected class BackupFileSystemWrapper : FileSystemWrapper
    {
        // TODO-UPDATE: Implement
        public override Stream? ReadFile(string filePath) => null;
        public override IEnumerable<string> GetFiles(string dirPath) => Enumerable.Empty<string>();
        public override IEnumerable<string> GetDirectories(string dirPath) => Enumerable.Empty<string>();
    }
}