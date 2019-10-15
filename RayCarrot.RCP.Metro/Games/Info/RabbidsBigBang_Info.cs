using System.Collections.Generic;

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
        protected override IList<BackupDir> GetBackupDirectories => RCPWinStoreGame.GetWinStoreBackupDirs(Game.GetManager<RCPWinStoreGame>(GameType.WinStore).FullPackageName);

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RabbidsBigBang;

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
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, Game.GetManager<RCPWinStoreGame>(GameType.WinStore).GetStorePageURI())
        };

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        #endregion
    }
}