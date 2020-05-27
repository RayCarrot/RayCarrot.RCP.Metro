﻿namespace RayCarrot.RCP.Metro
{
    // IDEA: Add backup info

    /// <summary>
    /// The Rayman Premiers Clics game info
    /// </summary>
    public sealed class RaymanPremiersClics_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanPremiersClics;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Other;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Premiers Clics";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => null;

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "RAYMAN.exe";

        /// <summary>
        /// Indicates if the game can be located. If set to false the game is required to be downloadable.
        /// </summary>
        public override bool CanBeLocated => true;

        #endregion
    }
}