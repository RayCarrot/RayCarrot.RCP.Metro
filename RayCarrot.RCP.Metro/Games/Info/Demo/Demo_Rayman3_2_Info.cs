using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 3 Demo 2 game info
    /// </summary>
    public sealed class Demo_Rayman3_2_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman3_2;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Demo;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman 3 Demo 2";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 3 Demo 2";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "MainP5Pvf.exe";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_R3Demo2_Url),
        };

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new Ray_M_Arena_3_Config(new Rayman3ConfigViewModel(Game));

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "R3_Setup_DX8D.exe")
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