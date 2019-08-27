using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Extensions;
using RayCarrot.IO;

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

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates a new instance of <see cref="BaseBackupInfo"/> from an <see cref="EducationalDosBoxGameInfo"/>
        /// </summary>
        /// <param name="gameInfos">The game infos, all having the same ID</param>
        /// <returns>The backup info</returns>
        public static BaseBackupInfo FromEducationalDosGameInfos(EducationalDosBoxGameInfo[] gameInfos)
        {
            if (gameInfos.Any(x => x.ID != gameInfos.First().ID))
                throw new Exception("The educational games must share the same ID to be used for the same backup info");

            var backupName = $"Educational Games - {gameInfos.First().ID}";

            return new BaseBackupInfo(RCFRCP.App.GetCompressedBackupFile(backupName),
                RCFRCP.App.GetBackupDir(backupName),
                new List<BackupDir>()
                {
                    new BackupDir()
                    {
                        DirPath = gameInfos.First().InstallDir,
                        SearchOption = SearchOption.TopDirectoryOnly,
                        ExtensionFilter = "*.sav",
                        ID = "0"
                    },
                    new BackupDir()
                    {
                        DirPath = gameInfos.First().InstallDir,
                        SearchOption = SearchOption.TopDirectoryOnly,
                        ExtensionFilter = "*.cfg",
                        ID = "1"
                    },
                }, gameInfos.JoinItems(Environment.NewLine, x => x.Name));
        }

        /// <summary>
        /// Creates a new instance of <see cref="BaseBackupInfo"/> from a <see cref="Games"/>
        /// </summary>
        /// <param name="game">The game</param>
        /// <returns>The backup info</returns>
        public static BaseBackupInfo FromGame(Games game)
        {
            var backupName = game.GetBackupName();
            var backupInfo = game.GetBackupInfo();

            if (backupInfo == null)
                return null;

            return new BaseBackupInfo(RCFRCP.App.GetCompressedBackupFile(backupName),
                RCFRCP.App.GetBackupDir(backupName),
                backupInfo,
                game.GetDisplayName());
        }

        #endregion
    }
}