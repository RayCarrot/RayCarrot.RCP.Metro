using System.Collections.Generic;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 3 (Win32) game manager
    /// </summary>
    public sealed class Rayman3_Win32 : RCPWin32Game
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman3;

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
            new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--3--hoodlum-havoc/5800b15eef3aa5ab3e8b4567.html")
        };

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(R3UbiIniHandler.SectionName, "Rayman 3", new string[]
        {
            "Rayman 3",
            "Rayman: 3",
            "Rayman 3 - Hoodlum Havoc",
            "Rayman: 3 - Hoodlum Havoc",
        });

        #endregion
    }
}