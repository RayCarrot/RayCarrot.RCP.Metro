#nullable disable
using System.Collections.Generic;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (Win32) game manager
/// </summary>
public sealed class GameManager_Rayman3_Win32 : GameManager_Win32
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Rayman3;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(UbiIniData_Rayman3.SectionName, "Rayman 3", new string[]
    {
        "Rayman 3",
        "Rayman: 3",
        "Rayman 3 - Hoodlum Havoc",
        "Rayman: 3 - Hoodlum Havoc",
    });

    #endregion
}