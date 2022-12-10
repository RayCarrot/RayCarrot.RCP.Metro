using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for an emulated MS-DOS program
/// </summary>
public abstract class MsDosGameDescriptor : EmulatedGameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.MsDos;

    /// <summary>
    /// Indicates if the game requires a disc to be mounted in order to play
    /// </summary>
    public virtual bool RequiresDisc => true;

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

    public virtual string? GetLaunchArgs(GameInstallation gameInstallation) => null;

    #endregion
}