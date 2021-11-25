#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids Demo (Win32) game manager
/// </summary>
public sealed class GameManager_RaymanRavingRabbidsDemo_Win32 : GameManager_Win32
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_RaymanRavingRabbids;

    public override string GetLaunchArgs => "/B Rayman4.bf";

    #endregion
}