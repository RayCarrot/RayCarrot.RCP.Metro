namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 3 (DOSBox) game manager
/// </summary>
public sealed class GameManager_Rayman1Demo3_DOSBox : GameManager_DOSBox
{
    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_Rayman1_3;

    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameDescriptor.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public override string ExecutableName => "RAYMAN.EXE";

    /// <summary>
    /// Indicates if the game requires a disc to be mounted in order to play
    /// </summary>
    public override bool RequiresMounting => false;

    #endregion
}