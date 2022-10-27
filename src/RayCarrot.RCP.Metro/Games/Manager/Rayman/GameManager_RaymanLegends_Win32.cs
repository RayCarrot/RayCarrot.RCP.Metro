#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

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
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(null, "Rayman Legends", new string[]
    {
        "Rayman Legends",
        "Rayman: Legends",
    });

    #endregion
}