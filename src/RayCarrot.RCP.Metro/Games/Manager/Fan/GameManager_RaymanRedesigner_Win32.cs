#nullable disable
using System.Collections.Generic;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman ReDesigner (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanRedesigner_Win32 : GameManager_Win32
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanRedesigner;

    #endregion
}