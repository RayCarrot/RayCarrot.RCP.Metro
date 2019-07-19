using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game backup item
    /// </summary>
    public class GameBackupItemViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public GameBackupItemViewModel(Games game)
        {
            Game = game;
            IconSource = game.GetIconSource();
            DisplayName = game.GetDisplayName();

            // If the type if DOSBox, check if GOG cloud sync is being used
            if (Game.GetInfo().GameType == GameType.DosBox)
            {
                try
                {
                    var cloudSyncDir = Game.GetInfo().InstallDirectory.Parent + "cloud_saves";
                    IsGOGCloudSyncUsed = cloudSyncDir.DirectoryExists && Directory.GetFileSystemEntries(cloudSyncDir).Any();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting if DOSBox game is using GOG cloud sync");
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

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

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
                FileSystemPath? backupLocation = Game.GetExistingBackup();

                if (backupLocation == null)
                    return;

                // Get the backup date
                LastBackup = backupLocation.Value.GetFileSystemInfo().CreationTime;

                // Get the backup size
                BackupSize = backupLocation.Value.GetSize();

                CanRestore = true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting backup info", Game);
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.ReadingBackupError, Game.GetDisplayName()), MessageType.Error);
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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.Backup_GOGSyncWarning, Resources.Backup_GOGSyncWarningHeader, MessageType.Warning);

            try
            {
                PerformingBackupRestore = true;

                // Confirm restore
                if (!await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Confirm, Game.GetDisplayName()), Resources.Restore_ConfirmHeader, MessageType.Warning, true))
                {
                    RCFCore.Logger?.LogInformationSource($"Restore canceled");

                    return;
                }

                ShowBackupRestoreIndicator = true;

                // Perform the restore
                if (await Task.Run(async () => await RCFRCP.Backup.RestoreAsync(Game)))
                {
                    ShowBackupRestoreIndicator = false;

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Restore_Success, Game.GetDisplayName()), Resources.Restore_SuccessHeader);
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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.Backup_GOGSyncWarning, Resources.Backup_GOGSyncWarningHeader, MessageType.Warning);

            try
            {
                PerformingBackupRestore = true;

                // Confirm backup if one already exists
                if ((Game.GetExistingBackup()?.Exists ?? false) && !await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Confirm, Game.GetDisplayName()), Resources.Backup_ConfirmHeader, MessageType.Warning, true))
                {
                    RCFCore.Logger?.LogInformationSource($"Backup canceled");
                    return;
                }

                ShowBackupRestoreIndicator = true;

                // Perform the backup
                if (await Task.Run(async () => await RCFRCP.Backup.BackupAsync(Game)))
                {
                    ShowBackupRestoreIndicator = false;

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Backup_Success, Game.GetDisplayName()), Resources.Backup_SuccessHeader);
                }

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
}