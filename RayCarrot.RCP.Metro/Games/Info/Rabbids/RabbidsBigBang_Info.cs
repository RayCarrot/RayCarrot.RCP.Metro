using System.Collections.Generic;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rabbids Big Bang game info
    /// </summary>
    public sealed class RabbidsBigBang_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => RCPWinStoreGame.GetWinStoreBackupDirs(Game.GetManager<RCPWinStoreGame>().FullPackageName);

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RabbidsBigBang;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rabbids;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rabbids Big Bang";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rabbids Big Bang";

        /// <summary>
        /// Gets the default file name for launching the game, if available
        /// </summary>
        public override string DefaultFileName => "Template.exe";

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        #endregion
    }
}