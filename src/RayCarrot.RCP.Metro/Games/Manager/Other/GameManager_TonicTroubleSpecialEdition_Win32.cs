﻿#nullable disable

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Tonic Trouble Special Edition (Win32) game manager
/// </summary>
public sealed class GameManager_TonicTroubleSpecialEdition_Win32 : GameManager_Win32
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.TonicTroubleSpecialEdition;

    /// <summary>
    /// Gets the launch arguments for the game
    /// </summary>
    public override string GetLaunchArgs => "-cdrom:";

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem("TONICT", "Tonic Trouble", new string[]
    {
        "Tonic Trouble",
    });

    #endregion
}