using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines info for a backup/restore operation
    /// </summary>
    public interface IGameBackups_BackupInfo
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
        /// The game display name
        /// </summary>
        string GameDisplayName { get; }

        /// <summary>
        /// The existing backups for this game, ordered by priority.
        /// The first backup in the collection is always the one to restore.
        /// </summary>
        GameBackups_ExistingBackup[] ExistingBackups { get; set; }

        /// <summary>
        /// The backup directories to use when performing a backup
        /// </summary>
        GameBackups_Directory[] BackupDirectories { get; set; }

        /// <summary>
        /// The backup directories to use when performing a restore
        /// </summary>
        GameBackups_Directory[] RestoreDirectories { get; set; }

        /// <summary>
        /// The latest available backup version
        /// </summary>
        int LatestAvailableBackupVersion { get; }

        /// <summary>
        /// The latest available backup version to restore
        /// </summary>
        int LatestAvailableRestoreVersion { get; set; }

        /// <summary>
        /// Refreshes the backup info
        /// </summary>
        /// <returns>The task</returns>
        Task RefreshAsync();
    }
}