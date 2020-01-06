using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman by his Fans game info
    /// </summary>
    public sealed class RaymanByHisFans_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            // NOTE: Due to a mistake where the .sct files were not included in previous backups we need to keep this version for legacy support
            new BackupDir(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
            new BackupDir(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "0", 1),
            new BackupDir(Game.GetInstallDir() + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 1),
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanByHisFans;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman by his Fans";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman by his Fans";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "rayfan.bat";

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new DosBoxConfig(Game);

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        #endregion
    }
}