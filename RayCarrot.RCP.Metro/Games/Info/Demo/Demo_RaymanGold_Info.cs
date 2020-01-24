using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Gold Demo game info
    /// </summary>
    public sealed class Demo_RaymanGold_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_RaymanGold;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Demo;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Gold Demo";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Gold Demo";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "RAYKIT.bat";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_RGoldDemo_Url),
        };

        /// <summary>
        /// The type of game if it can be downloaded
        /// </summary>
        public override GameType DownloadType => GameType.DosBox;

        // TODO: Implement + set that this does not need mounting
        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        //public override FrameworkElement ConfigUI => new DosBoxConfig(Game);

        #endregion
    }
}