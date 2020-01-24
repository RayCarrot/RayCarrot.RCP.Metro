using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman M Demo game info
    /// </summary>
    public sealed class Demo_RaymanM_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new List<BackupDir>()
        {
            new BackupDir(Game.GetInstallDir() + "Menu" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_RaymanM;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Demo;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman M Demo";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman M Demo";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "RaymanM.exe";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_RMDemo_Url),
        };

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new Ray_M_Arena_3_Config(new RaymanMDemoConfigViewModel());

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "RM_Setup_DX8.exe")
        };

        /// <summary>
        /// The group names to use for the options, config and utility dialog
        /// </summary>
        public override IEnumerable<string> DialogGroupNames => new string[]
        {
            UbiIniFileGroupName
        };

        #endregion
    }
}