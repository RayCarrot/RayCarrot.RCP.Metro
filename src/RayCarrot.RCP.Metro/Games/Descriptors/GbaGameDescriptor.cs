namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a GBA program
/// </summary>
public abstract class GbaGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Gba;
    public override bool DefaultToUseGameClient => true;

    #endregion
}