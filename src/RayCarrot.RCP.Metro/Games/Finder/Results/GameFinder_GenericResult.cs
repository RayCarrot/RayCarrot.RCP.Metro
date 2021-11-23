using System;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A finder result
/// </summary>
public class GameFinder_GenericResult : GameFinder_BaseResult
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="installLocation">The install location</param>
    /// <param name="handledAction">An optional action to add when the item gets handled</param>
    /// <param name="handledParameter">Optional parameter for the <see cref="GameFinder_BaseResult.HandledAction"/></param>
    /// <param name="displayName">The found item display name</param>
    public GameFinder_GenericResult(FileSystemPath installLocation, Action<FileSystemPath, object> handledAction, object handledParameter, string displayName) : base(installLocation, handledAction, handledParameter, displayName)
    { }

    /// <summary>
    /// Handles the found item
    /// </summary>
    /// <returns>The task</returns>
    public override Task HandleItemAsync()
    {
        // Call optional found action
        HandledAction?.Invoke(InstallLocation, HandledParameter);

        return Task.CompletedTask;
    }
}