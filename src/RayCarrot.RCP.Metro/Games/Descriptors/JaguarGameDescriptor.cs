namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Jaguar program
/// </summary>
public abstract class JaguarGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Jaguar;
    public override bool DefaultToUseGameClient => true;

    #endregion
}