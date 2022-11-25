namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game finder result
/// </summary>
public class GameFinder_GameResult : GameFinder_BaseResult
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameDescriptor">The game descriptor</param>
    /// <param name="installLocation">The install location</param>
    public GameFinder_GameResult(
        GameDescriptor gameDescriptor, 
        FileSystemPath installLocation) 
        : base(installLocation, gameDescriptor.DisplayName)
    {
        GameDescriptor = gameDescriptor;
    }

    /// <summary>
    /// The game descriptor
    /// </summary>
    public GameDescriptor GameDescriptor { get; }
}