using System.Collections.Generic;
using System.IO;
using System.Windows;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 60 Levels game info
    /// </summary>
    public sealed class GameInfo_Rayman60Levels : GameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<GameBackups_Directory> GetBackupDirectories => new GameBackups_Directory[]
        {
            new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
            new GameBackups_Directory(Game.GetInstallDir() + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0)
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman60Levels;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman 60 Levels";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 60 Levels";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman.bat";

        /// <summary>
        /// The config page view model, if any is available
        /// </summary>
        public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanByHisFans_ViewModel(Game);

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public override FrameworkElement OptionsUI => new GameOptions_DOSBox_UI(Game);

        /// <summary>
        /// The progression view model, if any is available
        /// </summary>
        public override GameProgression_BaseViewModel ProgressionViewModel => new GameProgression_RaymanDesigner_ViewModel(Game);

        /// <summary>
        /// Optional RayMap URL
        /// </summary>
        public override string RayMapURL => AppURLs.GetRay1MapGameURL("Rayman60LevelsPC", "r1/pc_60n");

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        /// <summary>
        /// Indicates if the game has archives which can be opened
        /// </summary>
        public override bool HasArchives => true;

        /// <summary>
        /// Gets the archive data manager for the game
        /// </summary>
        public override IArchiveDataManager GetArchiveDataManager => new Ray1PCArchiveDataManager(new Ray1PCArchiveConfigViewModel(Ray1Settings.GetDefaultSettings(Ray1Game.RayKit, Platform.PC)));

        /// <summary>
        /// Gets the archive file paths for the game
        /// </summary>
        /// <param name="installDir">The game's install directory</param>
        public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

        /// <summary>
        /// An optional emulator to use for the game
        /// </summary>
        public override Emulator Emulator => new Emulator_DOSBox(Game, GameType.DosBox);

        #endregion
    }
}