#nullable disable
using RayCarrot.IO;

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
    /// <param name="parameter">Optional parameter when the item is handled</param>
    public GameFinder_FoundResult(FileSystemPath installDir, object parameter)
    {
        InstallDir = installDir;
        Parameter = parameter;
    }

    /// <summary>
    /// The install directory
    /// </summary>
    public FileSystemPath InstallDir { get; }

    /// <summary>
    /// Optional parameter when the item is handled
    /// </summary>
    public object Parameter { get; }
}