using System;
using System.Collections.Generic;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Bowling 2 game info
    /// </summary>
    public sealed class RaymanBowling2_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman_Bowling_2_New_format", SearchOption.AllDirectories, "*", "0", 0),
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanBowling2;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Fan;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Bowling 2";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Bowling 2";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman Bowling 2.exe";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => false;

        #endregion
    }
}