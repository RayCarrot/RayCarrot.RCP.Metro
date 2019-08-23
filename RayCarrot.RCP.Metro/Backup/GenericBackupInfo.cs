using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Backup information for a generic game
    /// </summary>
    public class GenericBackupInfo : IBackupInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to get the information from</param>
        public GenericBackupInfo(Games game)
        {
            var backupName = game.GetBackupName();
            CompressedBackupLocation = RCFRCP.App.GetCompressedBackupFile(backupName);
            BackupLocation = RCFRCP.App.GetBackupDir(backupName);
            GameDisplayName = game.GetDisplayName();
            BackupDirectories = game.GetBackupInfo();
        }

        /// <summary>
        /// The path for the compressed backup
        /// </summary>
        public FileSystemPath CompressedBackupLocation { get; }

        /// <summary>
        /// The path for the backup
        /// </summary>
        public FileSystemPath BackupLocation { get; }

        /// <summary>
        /// The path for an existing backup, if one exists, otherwise null
        /// </summary>
        public FileSystemPath? ExistingBackupLocation => RCFRCP.App.GetExistingBackup(CompressedBackupLocation, BackupLocation);

        /// <summary>
        /// The game display name
        /// </summary>
        public string GameDisplayName { get; }

        /// <summary>
        /// The backup directories
        /// </summary>
        public List<BackupDir> BackupDirectories { get; }
    }
}