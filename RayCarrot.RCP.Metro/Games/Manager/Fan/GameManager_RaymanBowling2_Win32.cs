using System.Collections.Generic;
using MahApps.Metro.IconPacks;
using RayCarrot.Logging;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Bowling 2 (Win32) game manager
    /// </summary>
    public sealed class GameManager_RaymanBowling2_Win32 : GameManager_Win32
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanBowling2;

        /// <summary>
        /// Gets the purchase links for the game for this type
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/rayman_bowling_2/532563", PackIconMaterialKind.Earth), 
        };

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
        {
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenGameJoltPage, PackIconMaterialKind.Earth, new AsyncRelayCommand(async () =>
            {
                (await RCPServices.File.LaunchFileAsync("https://gamejolt.com/games/rayman_bowling_2/532563"))?.Dispose();
                RL.Logger?.LogTraceSource($"The game {Game} GameJolt page was opened");
            })),
        };

        #endregion
    }
}