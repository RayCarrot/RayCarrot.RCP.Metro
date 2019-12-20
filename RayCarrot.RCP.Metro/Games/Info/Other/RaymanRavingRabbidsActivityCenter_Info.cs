using System;
using System.Collections.Generic;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    // TODO: Add backup info

    /// <summary>
    /// The Rayman Raving Rabbids Activity Center game info
    /// </summary>
    public sealed class RaymanRavingRabbidsActivityCenter_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanRavingRabbidsActivityCenter;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Other;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Raving Rabbids Activity Center";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => null;

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman.exe";

        /// <summary>
        /// Indicates if the game can be located. If set to false the game is required to be downloadable.
        /// </summary>
        public override bool CanBeLocated => false;

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_RavingRabbidsActivityCenter_Url)
        };

        #endregion
    }
}