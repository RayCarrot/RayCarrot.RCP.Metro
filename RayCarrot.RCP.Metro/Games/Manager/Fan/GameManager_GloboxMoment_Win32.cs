﻿using System;
using System.Collections.Generic;
using MahApps.Metro.IconPacks;
using NLog;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Globox Moment (Win32) game manager
    /// </summary>
    public sealed class GameManager_GloboxMoment_Win32 : GameManager_Win32
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.GloboxMoment;

        /// <summary>
        /// Gets the purchase links for the game for this type
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_GameJolt, "https://gamejolt.com/games/globoxmoment/428585", PackIconMaterialKind.Earth),
        };

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
        {
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenGameJoltPage, PackIconMaterialKind.Earth, new AsyncRelayCommand(async () =>
            {
                (await RCPServices.File.LaunchFileAsync("https://gamejolt.com/games/globoxmoment/428585"))?.Dispose();
                Logger.Trace("The game {0} GameJolt page was opened", Game);
            })),
        };

        #endregion
    }
}