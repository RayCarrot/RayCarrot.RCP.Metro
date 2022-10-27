#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanOrigins_Win32 : GameManager_Win32
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanOrigins;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(null, "Rayman Origins", new string[]
    {
        "Rayman Origins",
        "Rayman: Origins",
    });

    #endregion
}