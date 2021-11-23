using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.IO;
using NLog;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a game backup item
/// </summary>
public class Page_Backup_ItemViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    /// <param name="backupInfo">The backup info</param>
    public Page_Backup_ItemViewModel(Games game, IGameBackups_BackupInfo backupInfo)
    {
        // Get the info
        var gameInfo = game.GetGameInfo();

        LatestAvailableBackupVersion = backupInfo.LatestAvailableBackupVersion;
        BackupInfo = backupInfo;
        IconSource = gameInfo.IconSource;
        DisplayName = backupInfo.GameDisplayName;

        // If the type if DOSBox, check if GOG cloud sync is being used
        if (game.GetGameType() == GameType.DosBox)
        {
            try
            {
                var cloudSyncDir = game.GetInstallDir(false).Parent + "cloud_saves";
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

        RestoreCommand = new AsyncRelayCommand(RestoreAsync);
        BackupCommand = new AsyncRelayCommand(BackupAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The backup info
    /// </summary>
    public IGameBackups_BackupInfo BackupInfo { get; }

    /// <summary>
    /// Indicates if there is a backup available to restore from
    /// </summary>
    public bool CanRestore { get; set; }

    /// <summary>
    /// The game icon source
    /// </summary>
    public string IconSource { get; }

    /// <summary>
    /// The game display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The last backup date
    /// </summary>
    public DateTime LastBackup { get; set; }

    /// <summary>
    /// The last backup display date
    /// </summary>
    public string LastBackupDisplay => LastBackup.ToShortDateString();

    /// <summary>
    /// The last backup size
    /// </summary>
    public ByteSize BackupSize { get; set; }

    /// <summary>
    /// Indicates if a backup or restore is being performed
    /// </summary>
    public bool PerformingBackupRestore { get; set; }

    /// <summary>
    /// Indicates if the indicator for a running backup or restore operation should show
    /// </summary>
    public bool ShowBackupRestoreIndicator { get; set; }

    /// <summary>
    /// Indicates if GOG cloud sync is enabled
    /// </summary>
    public bool IsGOGCloudSyncUsed { get; }

    /// <summary>
    /// The latest available backup version
    /// </summary>
    public int LatestAvailableBackupVersion { get; }

    /// <summary>
    /// The version of the backup
    /// </summary>
    public int BackupVersion { get; set; }

    /// <summary>
    /// Indicates if the backup is compressed
    /// </summary>
    public bool IsCompressed { get; set; }

    /// <summary>
    /// Debug info to show
    /// </summary>
    public string DebugInfo { get; set; }

    ///// <summary>
    ///// Indicates if the backed up files differ from the source files
    ///// </summary>
    //public bool RequiresBackup { get; set; }

    #endregion

    #region Commands

    public ICommand RestoreCommand { get; }

    public ICommand BackupCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the item
    /// </summary>
    /// <returns>The task</returns>
    public async Task RefreshAsync()
    {
        try
        {
            var backupLocation = BackupInfo.ExistingBackups.FirstOrDefault();

            if (backupLocation == null)
            {
                // NOTE: Not localized
                DebugInfo = $"LatestBackupVersion = {LatestAvailableBackupVersion}";

                return;
            }

            // Get the backup date
            LastBackup = backupLocation.Path.GetFileSystemInfo().LastWriteTime;

            // Get the backup size
            BackupSize = backupLocation.Path.GetSize();

            BackupVersion = backupLocation.BackupVersion;
            IsCompressed = backupLocation.IsCompressed;

            // NOTE: Not localized
            DebugInfo = $"IsCompressed = {IsCompressed}" +
                        $"{Environment.NewLine}" +
                        $"BackupVersion = {BackupVersion}" +
                        $"{Environment.NewLine}" +
                        $"LatestBackupVersion = {LatestAvailableBackupVersion}";

            CanRestore = true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting backup info");

            CanRestore = false;

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.ReadingBackupError, BackupInfo.GameDisplayName));
        }
    }

    /// <summary>
    /// Restores the previous backup
    /// </summary>
    /// <returns>The task</returns>
    public async Task RestoreAsync()
    {
        if (PerformingBackupRestore)
            return;

        // Show a warning message if GOG cloud sync is being used for this game as that will redirect the game data to its own directory
        if (IsGOGCloudSyncUsed)
            await Services.MessageUI.DisplayMessageAsync(Resources.Backup_GOGSyncWarning, Resources.Backup_GOGSyncWarningHeader, MessageType.Warning);

        try
        {
            PerformingBackupRestore = true;

            // Refresh the backup info
            await BackupInfo.RefreshAsync();

            // Confirm restore
            if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Confirm, BackupInfo.GameDisplayName), Resources.Restore_ConfirmHeader, MessageType.Warning, true))
            {
                Logger.Info("Restore canceled");

                return;
            }

            ShowBackupRestoreIndicator = true;

            // Perform the restore
            if (await Task.Run(async () => await Services.Backup.RestoreAsync(BackupInfo)))
            {
                ShowBackupRestoreIndicator = false;

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Restore_Success, BackupInfo.GameDisplayName), Resources.Restore_SuccessHeader);
            }
        }
        finally
        {
            PerformingBackupRestore = false;
            ShowBackupRestoreIndicator = false;
        }
    }

    /// <summary>
    /// Performs a new backup
    /// </summary>
    /// <returns>The task</returns>
    public async Task BackupAsync()
    {
        if (PerformingBackupRestore)
            return;

        // Show a warning message if GOG cloud sync is being used for this game as that will redirect the game data to its own directory
        if (IsGOGCloudSyncUsed)
            await Services.MessageUI.DisplayMessageAsync(Resources.Backup_GOGSyncWarning, Resources.Backup_GOGSyncWarningHeader, MessageType.Warning);

        try
        {
            PerformingBackupRestore = true;

            // Refresh the backup info
            await BackupInfo.RefreshAsync();

            // Confirm backup if one already exists
            if (BackupInfo.ExistingBackups.Any() && !await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Confirm, BackupInfo.GameDisplayName), Resources.Backup_ConfirmHeader, MessageType.Warning, true))
            {
                Logger.Info("Backup canceled");
                return;
            }

            ShowBackupRestoreIndicator = true;

            // Perform the backup
            if (await Task.Run(async () => await Services.Backup.BackupAsync(BackupInfo)))
            {
                ShowBackupRestoreIndicator = false;

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Backup_Success, BackupInfo.GameDisplayName), Resources.Backup_SuccessHeader);
            }

            // Refresh the backup info
            await BackupInfo.RefreshAsync();

            // Refresh the item
            await RefreshAsync();
        }
        finally
        {
            PerformingBackupRestore = false;
            ShowBackupRestoreIndicator = false;
        }
    }

    #endregion
}