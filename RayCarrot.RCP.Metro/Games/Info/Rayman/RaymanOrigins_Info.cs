using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Origins game info
    /// </summary>
    public sealed class RaymanOrigins_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins", SearchOption.AllDirectories, "*", "0", 0)
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanOrigins;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Origins";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Origins";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman Origins.exe";

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new Ray_Origins_Legends_Config(Game);

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        #endregion
    }
}