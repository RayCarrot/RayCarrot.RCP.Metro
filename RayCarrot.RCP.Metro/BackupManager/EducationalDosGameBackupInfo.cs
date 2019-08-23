using System.Collections.Generic;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Backup information for an educational DOS game
    /// </summary>
    public class EducationalDosGameBackupInfo : IBackupInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="gameInfo">The game info to get the information from</param>
        public EducationalDosGameBackupInfo(EducationalDosBoxGameInfo gameInfo)
        {
            var backupName = $"Educational Games - {gameInfo.ID}";
            CompressedBackupLocation = RCFRCP.App.GetCompressedBackupFile(backupName);
            BackupLocation = RCFRCP.App.GetBackupDir(backupName);
            GameDisplayName = gameInfo.Name;
            BackupDirectories = new List<BackupDir>()
            {
                new BackupDir()
                {
                    DirPath = gameInfo.InstallDIr,
                    SearchOption = SearchOption.TopDirectoryOnly,
                    ExtensionFilter = "*.sav",
                    ID = "0"
                },
                new BackupDir()
                {
                    DirPath = gameInfo.InstallDIr,
                    SearchOption = SearchOption.TopDirectoryOnly,
                    ExtensionFilter = "*.cfg",
                    ID = "1"
                },
            };
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