namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a PS1 program
/// </summary>
public abstract class Ps1GameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Ps1;
    public override bool DefaultToUseGameClient => true;

    #endregion
}