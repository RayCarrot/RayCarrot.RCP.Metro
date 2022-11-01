namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result for the <see cref="GameFinder_BaseItem.CustomFinderAction"/>
/// </summary>
public class GameFinder_FoundResult
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="installDir">The install directory</param>
    public GameFinder_FoundResult(FileSystemPath installDir)
    {
        InstallDir = installDir;
    }

    /// <summary>
    /// The install directory
    /// </summary>
    public FileSystemPath InstallDir { get; }
}