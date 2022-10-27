#nullable disable
using System.Collections.Generic;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (Win32) game manager
/// </summary>
public sealed class GameManager_Rayman2_Win32 : GameManager_Win32
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Rayman2;

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(UbiIniData_Rayman2.SectionName, "Rayman 2", new string[]
    {
        "Rayman 2",
        "Rayman: 2",
        "Rayman 2 - The Great Escape",
        "Rayman: 2 - The Great Escape",
        "GOG.com Rayman 2",
    });

    #endregion
}