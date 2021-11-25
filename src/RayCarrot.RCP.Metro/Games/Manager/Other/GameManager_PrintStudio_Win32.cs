#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Print Studio (Win32) game manager
/// </summary>
public sealed class GameManager_PrintStudio_Win32 : GameManager_Win32
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.PrintStudio;

    #endregion
}