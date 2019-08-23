using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines info for a backup/restore operation
    /// </summary>
    public interface IBackupInfo
    {
        /// <summary>
        /// The path for the compressed backup
        /// </summary>
        FileSystemPath CompressedBackupLocation { get; }

        /// <summary>
        /// The path for the backup
        /// </summary>
        FileSystemPath BackupLocation { get; }

        /// <summary>
        /// The path for an existing backup, if one exists, otherwise null
        /// </summary>
        FileSystemPath? ExistingBackupLocation { get; }

        /// <summary>
        /// The game display name
        /// </summary>
        string GameDisplayName { get; }

        /// <summary>
        /// The backup directories
        /// </summary>
        List<BackupDir> BackupDirectories { get; }
    }
}