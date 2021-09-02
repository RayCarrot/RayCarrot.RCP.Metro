using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends (Win32) game manager
    /// </summary>
    public sealed class GameManager_RaymanLegends_Win32 : GameManager_Win32
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanLegends;

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--legends/56c4948888a7e300458b47da.html")
        };

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(null, "Rayman Legends", new string[]
        {
            "Rayman Legends",
            "Rayman: Legends",
        });

        #endregion
    }
}