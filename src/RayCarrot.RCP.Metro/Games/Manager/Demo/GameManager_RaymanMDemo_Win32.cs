#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M Demo (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanMDemo_Win32 : GameManager_Win32
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_RaymanM;

    #endregion
}