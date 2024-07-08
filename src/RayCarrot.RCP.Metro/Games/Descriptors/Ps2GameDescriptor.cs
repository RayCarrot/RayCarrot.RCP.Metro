namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a PS2 program
/// </summary>
public abstract class Ps2GameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Ps2;
    public override bool DefaultToUseGameClient => true;

    #endregion
}