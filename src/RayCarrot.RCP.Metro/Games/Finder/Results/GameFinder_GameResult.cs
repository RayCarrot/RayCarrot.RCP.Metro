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
    /// <param name="game">The game</param>
    /// <param name="installLocation">The install location</param>
    /// <param name="gameType">The game type</param>
    /// <param name="handledAction">An optional action to add when the item gets handled</param>
    /// <param name="handledParameter">Optional parameter for the <see cref="GameFinder_BaseResult.HandledAction"/></param>
    public GameFinder_GameResult(Games game, FileSystemPath installLocation, GameType gameType, Action<FileSystemPath, object> handledAction = null, object handledParameter = null) : base(installLocation, handledAction, handledParameter, game.GetGameDescriptor().DisplayName)
    {
        Game = game;
        GameType = gameType;
    }

    /// <summary>
    /// The game
    /// </summary>
    public Games Game { get; }

    /// <summary>
    /// The game type
    /// </summary>
    public GameType GameType { get; }

    /// <summary>
    /// Adds the found game and calls the optional <see cref="GameFinder_BaseResult.HandledAction"/>
    /// </summary>
    /// <returns>The task</returns>
    public override async Task HandleItemAsync()
    {
        // Call optional found action
        HandledAction?.Invoke(InstallLocation, HandledParameter);

        // Add the game
        await Services.Games.AddGameAsync(Game, GameType, InstallLocation, false);
    }
}