namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for an MS-DOS program
/// </summary>
public abstract class MsDosGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.MsDos;

    public override bool DefaultToUseGameClient => true;

    // TODO-14: Merge this and DefaultFileName. Don't use .bat files.
    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameDescriptor.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public abstract string ExecutableName { get; }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateGameAddAction(this),
    };

    #endregion
}