#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Coding (Win32) game manager
/// </summary>
public sealed class GameManager_RabbidsCoding_Win32 : GameManager_Win32
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RabbidsCoding;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(null, "Rabbids Coding", new string[]
    {
        "RabbidsCoding",
        "Rabbids Coding",
    });

    #endregion
}