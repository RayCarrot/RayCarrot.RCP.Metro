﻿#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends (Steam) game manager
/// </summary>
public sealed class GameManager_RaymanLegends_Steam : GameManager_Steam
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanLegends;

    /// <summary>
    /// Gets the Steam ID for the game
    /// </summary>
    public override string SteamID => "242550";

    #endregion
}