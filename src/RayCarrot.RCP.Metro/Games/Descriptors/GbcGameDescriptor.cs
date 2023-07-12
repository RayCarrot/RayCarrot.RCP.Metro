namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a GBC program
/// </summary>
public abstract class GbcGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Gbc;
    public override bool DefaultToUseGameClient => true;

    #endregion
}