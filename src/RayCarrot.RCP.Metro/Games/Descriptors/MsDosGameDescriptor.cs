namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for an MS-DOS program
/// </summary>
public abstract class MsDosGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.MsDos;
    public override bool DefaultToUseGameClient => true;

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateGameAddAction(this),
    };

    #endregion
}