using System;

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
    /// <param name="displayName">The found item display name</param>
    public GameFinder_GenericResult(
        FileSystemPath installLocation,
        Action<FileSystemPath>? handledAction,
        string displayName)
        : base(installLocation, displayName)
    {
        HandledAction = handledAction;
    }

    /// <summary>
    /// An optional action to add when the item gets handled
    /// </summary>
    public Action<FileSystemPath>? HandledAction { get; }
}