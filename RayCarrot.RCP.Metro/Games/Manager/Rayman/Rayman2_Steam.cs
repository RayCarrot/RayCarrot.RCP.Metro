using System.Collections.Generic;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 (Steam) game manager
    /// </summary>
    public sealed class Rayman2_Steam : RCPSteamGame
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman2;

        /// <summary>
        /// Gets the Steam ID for the game
        /// </summary>
        public override string SteamID => "15060";

        // Override the Steam purchase link
        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[0];

        #endregion
    }
}