namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base for a Rayman Control Panel game
/// </summary>
public abstract class BaseGameData
{
    #region Public Abstract Properties

    /// <summary>
    /// The game
    /// </summary>
    public abstract Games Game { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the game has been added
    /// </summary>
    public bool IsAdded => Game.IsAdded();

    #endregion
}