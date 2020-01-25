using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 Demo 2 game info
    /// </summary>
    public sealed class Demo_Rayman1_2_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman1_2;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Demo;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman 1 Demo 2";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 1 Demo 2";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "RAYMAN.EXE";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_R1Demo2_Url),
        };

        /// <summary>
        /// The type of game if it can be downloaded
        /// </summary>
        public override GameType DownloadType => GameType.DosBox;

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new DosBoxConfig(new Rayman1ConfigViewModel(Game, true));

        #endregion
    }
}