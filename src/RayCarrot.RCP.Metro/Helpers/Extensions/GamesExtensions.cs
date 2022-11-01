using System;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Games"/>
/// </summary>
public static class GamesExtensions
{
    // TODO-14: Remove most of these

    /// <summary>
    /// Determines if the specified game has been added to the program
    /// </summary>
    /// <param name="game">The game to check if it's added</param>
    /// <returns>True if the game has been added, otherwise false</returns>
    public static bool IsAdded(this Games game)
    {
        return Services.Data.Game_GameInstallations.Any(x => x.LegacyGame == game);
    }

    /// <summary>
    /// Gets the install directory for the game or an empty path if not found or if it doesn't exist
    /// </summary>
    /// <param name="game">The game to get the install directory for</param>
    /// <param name="throwIfNotFound">Indicates if an exception should be thrown if the directory is not found</param>
    /// <returns>The install directory or an empty path if not found or if it doesn't exist</returns>
    public static FileSystemPath GetInstallDir(this Games game, bool throwIfNotFound = true)
    {
        // Get the game installation
        GameInstallation? gameInstallation = Services.Data.Game_GameInstallations.FirstOrDefault(x => x.LegacyGame == game);

        // Make sure it's not null
        if (gameInstallation == null)
        {
            if (throwIfNotFound)
                throw new Exception($"The data for the requested game '{game}' could not be found");
            else
                return FileSystemPath.EmptyPath;
        }

        // Return the path
        return gameInstallation.InstallLocation;
    }

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
    public static GameInstallation GetInstallation(this Games game) => Services.Data.Game_GameInstallations.First(x => x.LegacyGame == game);
}