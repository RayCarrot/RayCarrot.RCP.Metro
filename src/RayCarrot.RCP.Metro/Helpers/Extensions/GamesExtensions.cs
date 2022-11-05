﻿using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Games"/>
/// </summary>
public static class GamesExtensions
{
    // TODO-14: Remove all of these

    public static bool IsAdded(this GameDescriptor gameDescriptor) =>
        Services.Data.Game_GameInstallations.Any(x => x.GameDescriptor == gameDescriptor);

    /// <summary>
    /// Gets the game info for the specified game
    /// </summary>
    /// <param name="game">The game to get the info for</param>
    /// <returns>The info</returns>
    public static GameDescriptor GetGameDescriptor(this Games game) => GetGameDescriptor<GameDescriptor>(game);

    /// <summary>
    /// Gets the game info for the specified game
    /// </summary>
    /// <param name="game">The game to get the info for</param>
    /// <returns>The info</returns>
    public static T GetGameDescriptor<T>(this Games game)
        where T : GameDescriptor
    {
        var g = Services.Games;
        return (T)g.EnumerateGameDescriptors().First(x => x.LegacyGame == game);
    }

    // TODO-14: Remove once no longer needed
    public static GameInstallation GetInstallation(this Games game) => 
        Services.Data.Game_GameInstallations.First(x => x.LegacyGame == game);
    public static GameInstallation GetInstallation(this GameDescriptor gameDescriptor) => 
        Services.Data.Game_GameInstallations.First(x => x.GameDescriptor == gameDescriptor);
}