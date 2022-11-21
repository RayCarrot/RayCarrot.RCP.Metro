using System;
using System.Threading.Tasks;

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
    /// <param name="handledAction">An optional action to add when the item gets handled</param>
    public GameFinder_GameResult(
        GameDescriptor gameDescriptor, 
        FileSystemPath installLocation, 
        Action<FileSystemPath>? handledAction = null) 
        : base(installLocation, handledAction, gameDescriptor.DisplayName)
    {
        GameDescriptor = gameDescriptor;
    }

    /// <summary>
    /// The game descriptor
    /// </summary>
    public GameDescriptor GameDescriptor { get; }

    public override async Task<GameInstallation?> HandleItemAsync()
    {
        // Call optional found action
        HandledAction?.Invoke(InstallLocation);

        // TODO-UPDATE: We want to be able to call AddGamesAsync to avoid multiple refreshes
        // Add the game
        return await Services.Games.AddGameAsync(GameDescriptor, InstallLocation);
    }
}