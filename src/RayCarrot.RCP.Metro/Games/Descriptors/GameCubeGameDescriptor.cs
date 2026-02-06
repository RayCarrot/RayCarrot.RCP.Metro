namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a GameCube program
/// </summary>
public abstract class GameCubeGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.GameCube;
    public override bool DefaultToUseGameClient => true;

    #endregion
}