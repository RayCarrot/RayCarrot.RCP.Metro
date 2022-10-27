#nullable disable
using System.Collections.Generic;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman The Dark  Magician's Reign of Terror (Win32) game manager
/// </summary>
public sealed class GameManager_TheDarkMagiciansReignofTerror_Win32 : GameManager_Win32
{
    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.TheDarkMagiciansReignofTerror;
}