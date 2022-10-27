#nullable disable
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanRavingRabbids_Win32 : GameManager_Win32
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanRavingRabbids;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(null, "Rayman Raving Rabbids", new string[]
    {
        "Rayman Raving Rabbids",
        "Rayman: Raving Rabbids",
        "RRR",
    });

    #endregion
}