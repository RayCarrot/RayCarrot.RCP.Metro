#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 60 Levels (DOSBox) game manager
/// </summary>
public sealed class GameManager_Rayman60Levels_DOSBox : GameManager_DOSBox
{
    #region Public Overrides

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Rayman60Levels;

    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameInfo.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public override string ExecutableName => "RAYPLUS.EXE";

    #endregion
}