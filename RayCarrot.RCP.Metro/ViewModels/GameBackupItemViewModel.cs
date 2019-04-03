using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.CarrotFramework;

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
                // Get the backup directory
                var dir = Game.GetBackupDir();

                // Make sure the directory exists or is not empty
                if (!dir.DirectoryExists || !Directory.GetFileSystemEntries(dir).Any())
                    return;

                // Get the backup date
                LastBackup = dir.GetDirectoryInfo().CreationTime;

                // Get the backup size
                BackupSize = dir.GetSize();

                CanRestore = true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting backup info", Game);
                await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.ReadingBackupError, Game.GetDisplayName()), MessageType.Error);
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

            try
            {
                PerformingBackupRestore = true;

                // Perform the restore
                await Task.Run(async () => await RCFRCP.Backup.RestoreAsync(Game));
            }
            finally
            {
                PerformingBackupRestore = false;
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

            try
            {
                PerformingBackupRestore = true;

                // Perform the backup
                await Task.Run(async () => await RCFRCP.Backup.BackupAsync(Game));

                // Refresh the item
                await RefreshAsync();
            }
            finally
            {
                PerformingBackupRestore = false;
            }
        }

        #endregion
    }
}