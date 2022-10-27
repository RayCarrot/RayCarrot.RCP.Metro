#nullable disable
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
    /// <param name="handledParameter">Optional parameter for the <see cref="GameFinder_BaseResult.HandledAction"/></param>
    public GameFinder_GameResult(
        GameDescriptor gameDescriptor, 
        FileSystemPath installLocation, 
        Action<FileSystemPath, object> handledAction = null, 
        object handledParameter = null) 
        : base(installLocation, handledAction, handledParameter, gameDescriptor.DisplayName)
    {
        GameDescriptor = gameDescriptor;
    }

    /// <summary>
    /// The game descriptor
    /// </summary>
    public GameDescriptor GameDescriptor { get; }

    // TODO-14: Find nicer solution to this
    public GameInstallation GameInstallation { get; private set; }

    /// <summary>
    /// Adds the found game and calls the optional <see cref="GameFinder_BaseResult.HandledAction"/>
    /// </summary>
    /// <returns>The task</returns>
    public override async Task HandleItemAsync()
    {
        // Call optional found action
        HandledAction?.Invoke(InstallLocation, HandledParameter);

        // Add the game
        GameInstallation = await Services.Games.AddGameAsync(GameDescriptor, InstallLocation, false);
    }
}