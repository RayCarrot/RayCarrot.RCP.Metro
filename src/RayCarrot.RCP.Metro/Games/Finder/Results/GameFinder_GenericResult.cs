using System;
using System.Threading.Tasks;

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
        : base(installLocation, handledAction, displayName)
    { }

    public override Task<GameInstallation?> HandleItemAsync()
    {
        // Call optional found action
        HandledAction?.Invoke(InstallLocation);

        return Task.FromResult<GameInstallation?>(null);
    }
}