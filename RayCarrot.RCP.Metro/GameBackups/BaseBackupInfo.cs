using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base backup info to use for <see cref="IBackupInfo"/>
    /// </summary>
    public class BaseBackupInfo : IBackupInfo
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="backupName">The backup name to use to get the paths</param>
        /// <param name="backupDirectories">The backup directory infos</param>
        /// <param name="displayName">The game display name</param>
        public BaseBackupInfo(string backupName, IEnumerable<BackupDir> backupDirectories, string displayName)
        {
            BackupName = backupName;
            AllBackupDirectories = backupDirectories.GroupBy(x => x.BackupVersion).ToDictionary(x => x.Key, x => x.ToArray());

            // Get the latest backup version to create a backup from
            LatestAvailableBackupVersion = AllBackupDirectories.Select(x => x.Value.Select(y => y.BackupVersion)).SelectMany(x => x).Max();

            BackupLocation = RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily + (BackupName + $"-{LatestAvailableBackupVersion.ToString().PadLeft(2, '0')}");
            CompressedBackupLocation = BackupLocation.FullPath + CommonPaths.BackupCompressionExtension;
            GameDisplayName = displayName;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The backup name to use to get the paths
        /// </summary>
        private string BackupName { get; }

        /// <summary>
        /// All backup directories
        /// </summary>
        private Dictionary<int, BackupDir[]> AllBackupDirectories { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path for the compressed backup
        /// </summary>
        public FileSystemPath CompressedBackupLocation { get; }

        /// <summary>
        /// The path for the backup
        /// </summary>
        public FileSystemPath BackupLocation { get; }

        /// <summary>
        /// The game display name
        /// </summary>
        public string GameDisplayName { get; }

        /// <summary>
        /// The existing backups for this game, ordered by priority.
        /// The first backup in the collection is always the one to restore.
        /// </summary>
        public RCPBackup[] ExistingBackups { get; set; }

        /// <summary>
        /// The backup directories to use when performing a backup
        /// </summary>
        public BackupDir[] BackupDirectories { get; set; }

        /// <summary>
        /// The backup directories to use when performing a restore
        /// </summary>
        public BackupDir[] RestoreDirectories { get; set; }

        /// <summary>
        /// The latest available backup version
        /// </summary>
        public int LatestAvailableBackupVersion { get; }

        /// <summary>
        /// The latest available backup version to restore
        /// </summary>
        public int LatestAvailableRestoreVersion { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get all existing backups for this info, ordered by priority
        /// </summary>
        /// <returns>The existing backups</returns>
        private async Task<RCPBackup[]> GetExistingBackupsAsync()
        {
            try
            {
                var path = RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily;

                if (!path.DirectoryExists)
                    return new RCPBackup[0];

                return Directory.GetFileSystemEntries(path, $"{BackupName}*", SearchOption.TopDirectoryOnly).
                    Select(x => new RCPBackup(x)).
                    OrderByDescending(x => x.BackupVersion).
                    ThenBy(x => x.IsCompressed == RCFRCP.Data.CompressBackups ? 0 : 1).
                    ToArray();
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting existing backups");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.GetExistingBackupsError, GameDisplayName));
                return new RCPBackup[0];
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the backup info
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            ExistingBackups = await GetExistingBackupsAsync();

            // Get the latest backup version to restore from
            LatestAvailableRestoreVersion = ExistingBackups.FirstOrDefault()?.BackupVersion ?? -1;

            BackupDirectories = AllBackupDirectories.TryGetValue(LatestAvailableBackupVersion) ?? new BackupDir[0];
            RestoreDirectories = LatestAvailableRestoreVersion == -1 ? new BackupDir[0] : AllBackupDirectories.TryGetValue(LatestAvailableRestoreVersion) ?? new BackupDir[0];
        }

        #endregion
    }
}