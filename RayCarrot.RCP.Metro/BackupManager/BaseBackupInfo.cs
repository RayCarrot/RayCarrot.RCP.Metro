using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base backup info to use for <see cref="IBackupInfo"/>
    /// </summary>
    public class BaseBackupInfo : IBackupInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="compressedBackupLocation"></param>
        /// <param name="backupLocation"></param>
        /// <param name="backupDirectories"></param>
        /// <param name="displayName"></param>
        public BaseBackupInfo(FileSystemPath compressedBackupLocation, FileSystemPath backupLocation, List<BackupDir> backupDirectories, string displayName)
        {
            CompressedBackupLocation = compressedBackupLocation;
            BackupLocation = backupLocation;
            GameDisplayName = displayName;
            BackupDirectories = backupDirectories;
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