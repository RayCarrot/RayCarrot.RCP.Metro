using System;
using System.Threading.Tasks;
using RayCarrot.IO;

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
    /// <param name="handledAction">An optional action to add when the item gets handled</param>
    /// <param name="handledParameter">Optional parameter for the <see cref="HandledAction"/></param>
    /// <param name="displayName">The found item display name</param>
    protected GameFinder_BaseResult(FileSystemPath installLocation, Action<FileSystemPath, object> handledAction, object handledParameter, string displayName)
    {
        InstallLocation = installLocation;
        HandledAction = handledAction;
        HandledParameter = handledParameter;
        DisplayName = displayName;
    }

    /// <summary>
    /// The install location
    /// </summary>
    public FileSystemPath InstallLocation { get; }

    /// <summary>
    /// Optional parameter for the <see cref="HandledAction"/>
    /// </summary>
    public object HandledParameter { get; }

    /// <summary>
    /// An optional action to add when the item gets handled
    /// </summary>
    public Action<FileSystemPath, object> HandledAction { get; }

    /// <summary>
    /// The found item display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Handles the found item
    /// </summary>
    /// <returns>The task</returns>
    public abstract Task HandleItemAsync();
}