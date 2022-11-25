namespace RayCarrot.RCP.Metro;

/// <summary>
/// A finder item result base
/// </summary>
public abstract class GameFinder_BaseResult
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="installLocation">The install location</param>
    /// <param name="displayName">The found item display name</param>
    protected GameFinder_BaseResult(
        FileSystemPath installLocation, 
        string displayName)
    {
        InstallLocation = installLocation;
        DisplayName = displayName;
    }

    /// <summary>
    /// The install location
    /// </summary>
    public FileSystemPath InstallLocation { get; }

    /// <summary>
    /// The found item display name
    /// </summary>
    public string DisplayName { get; }
}