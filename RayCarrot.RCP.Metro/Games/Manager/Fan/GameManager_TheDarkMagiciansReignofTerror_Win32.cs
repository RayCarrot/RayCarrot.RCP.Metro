﻿using System.Collections.Generic;
using MahApps.Metro.IconPacks;
using NLog;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman The Dark  Magician's Reign of Terror (Win32) game manager
    /// </summary>
    public sealed class GameManager_TheDarkMagiciansReignofTerror_Win32 : GameManager_Win32
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.TheDarkMagiciansReignofTerror;

        /// <summary>
        /// Gets the purchase links for the game for this type
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/Rayman_The_Dark_Magicians_Reign_of_terror/237701", PackIconMaterialKind.Earth), 
        };

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
        {
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenGameJoltPage, PackIconMaterialKind.Earth, new AsyncRelayCommand(async () =>
            {
                (await RCPServices.File.LaunchFileAsync("https://gamejolt.com/games/Rayman_The_Dark_Magicians_Reign_of_terror/237701"))?.Dispose();
                Logger.Trace($"The game {Game} GameJolt page was opened");
            })),
        };

        #endregion
    }
}