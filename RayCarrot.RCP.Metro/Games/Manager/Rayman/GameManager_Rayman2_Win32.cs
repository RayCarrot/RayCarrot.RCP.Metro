using System.Collections.Generic;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 (Win32) game manager
    /// </summary>
    public sealed class GameManager_Rayman2_Win32 : GameManager_Win32
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman2;

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_2_the_great_escape"),
            new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
        };

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(R2UbiIniHandler.SectionName, "Rayman 2", new string[]
        {
            "Rayman 2",
            "Rayman: 2",
            "Rayman 2 - The Great Escape",
            "Rayman: 2 - The Great Escape",
            "GOG.com Rayman 2",
        });

        #endregion
    }
}