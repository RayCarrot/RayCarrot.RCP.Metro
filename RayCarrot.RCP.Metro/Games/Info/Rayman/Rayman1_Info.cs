using System.Collections.Generic;
using System.IO;
using System.Windows;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 game info
    /// </summary>
    public sealed class Rayman1_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
            new BackupDir(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman1;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 1";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman.exe";

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new Ray_1_KIT_EDU_Config(new Rayman1ConfigViewModel(Game));

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public override FrameworkElement OptionsUI => new DOSBoxOptions(Game);

        /// <summary>
        /// The progression view model, if any is available
        /// </summary>
        public override BaseProgressionViewModel ProgressionViewModel => new Rayman1ProgressionViewModel();

        /// <summary>
        /// Optional RayMap URL
        /// </summary>
        public override string RayMapURL => CommonUrls.GetRay1MapGameURL("RaymanPC_1_21", "r1/pc_121");

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        /// <summary>
        /// An optional emulator to use for the game
        /// </summary>
        public override Emulator Emulator => new DOSBoxEmulator(Game, GameType.DosBox);

        #endregion
    }
}