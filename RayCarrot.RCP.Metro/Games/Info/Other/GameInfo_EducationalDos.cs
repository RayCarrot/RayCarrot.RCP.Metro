using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Educational Dos game info
    /// </summary>
    public sealed class GameInfo_EducationalDos : GameInfo
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.EducationalDos;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Other;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Educational Games";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => throw new Exception("A generic backup name can not be obtained for an educational DOS game due to it being a collection of multiple games");

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => RCPServices.Data.EducationalDosBoxGames?.FirstOrDefault()?.LaunchName;

        /// <summary>
        /// The config page view model, if any is available
        /// </summary>
        public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanEduDos_ViewModel(Game);

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public override FrameworkElement OptionsUI => new GameOptions_EducationalDos_UI();

        /// <summary>
        /// Optional RayMap URL
        /// </summary>
        public override string RayMapURL => AppURLs.GetRay1MapGameURL("RaymanEducationalPC", "r1/edu/pc_gb", "GB1");

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        public override IList<IGameBackups_BackupInfo> GetBackupInfos
        {
            get
            {
                // Get the games with a launch mode
                var games = RCPServices.Data.EducationalDosBoxGames.Where(x => !x.LaunchMode.IsNullOrWhiteSpace()).ToArray();

                // Return a collection of the backup infos for the available games
                return games.Select(x =>
                {
                    var backupName = $"Educational Games - {x.LaunchMode}";

                    return new GameBackups_BackupInfo(backupName,
                        new GameBackups_Directory[]
                        {
                            new GameBackups_Directory(x.InstallDir, SearchOption.TopDirectoryOnly, $"EDU{x.LaunchMode}??.SAV", "0", 0),
                            new GameBackups_Directory(x.InstallDir, SearchOption.TopDirectoryOnly, $"EDU{x.LaunchMode}.CFG", "1", 0)
                        }, x.Name);
                }).ToArray<IGameBackups_BackupInfo>();
            }
        }

        /// <summary>
        /// Indicates if the game has archives which can be opened
        /// </summary>
        public override bool HasArchives => true;

        /// <summary>
        /// Gets the archive data manager for the game
        /// </summary>
        public override IArchiveDataManager GetArchiveDataManager => new Ray1PCArchiveDataManager(new Ray1PCArchiveConfigViewModel(Ray1Settings.GetDefaultSettings(Ray1Game.RayEdu, Platform.PC)));

        /// <summary>
        /// Gets the archive file paths for the game
        /// </summary>
        /// <param name="installDir">The game's install directory</param>
        public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

        /// <summary>
        /// An optional emulator to use for the game
        /// </summary>
        public override Emulator Emulator => new Emulator_DOSBox(Game, GameType.EducationalDosBox);

        #endregion
    }
}