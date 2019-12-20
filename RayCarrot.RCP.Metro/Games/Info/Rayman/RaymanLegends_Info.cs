using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using RayCarrot.IO;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends game info
    /// </summary>
    public sealed class RaymanLegends_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends", SearchOption.AllDirectories, "*", "0", 0)
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanLegends;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Legends";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Legends";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman Legends.exe";

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