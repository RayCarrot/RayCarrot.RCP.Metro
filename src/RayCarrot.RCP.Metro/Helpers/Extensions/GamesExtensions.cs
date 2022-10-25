using System;
using System.Collections.Generic;
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
        return Services.Data.Game_GameInstallations.Any(x => x.Game == game);
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
        GameInstallation? gameInstallation = Services.Data.Game_GameInstallations.FirstOrDefault(x => x.Game == game);

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
    /// Gets the installed game type for the game
    /// </summary>
    /// <param name="game">The game to get the installed game type for</param>
    /// <returns>The game type</returns>
    public static GameType GetGameType(this Games game) => game.GetInstallation().GameType;

    /// <summary>
    /// Gets the game manager for the specified game with the current type
    /// </summary>
    /// <param name="game">The game to get the manager for</param>
    /// <returns>The manager</returns>
    public static GameManager GetManager(this Games game) => GetManager(game, game.GetGameType());

    /// <summary>
    /// Gets the available game managers for the specified game
    /// </summary>
    /// <param name="game">The game to get the managers for</param>
    /// <returns>The managers</returns>
    public static IEnumerable<GameManager> GetManagers(this Games game)
    {
        var g = Services.App.GamesManager;
        return g.GameManagers[game].Values.Select(managerType => g.CreateCachedInstance<GameManager>(managerType));
    }

    /// <summary>
    /// Gets the game manager for the specified game
    /// </summary>
    /// <param name="game">The game to get the manager for</param>
    /// <param name="type">The type of game to get the manager for</param>
    /// <returns>The manager</returns>
    public static GameManager GetManager(this Games game, GameType type)
    {
        var g = Services.App.GamesManager;
        return g.CreateCachedInstance<GameManager>(g.GameManagers[game][type]);
    }

    /// <summary>
    /// Gets the game manager of the specified type for the specified game
    /// </summary>
    /// <typeparam name="T">The type of manager to get</typeparam>
    /// <param name="game">The game to get the manager for</param>
    /// <param name="type">The type of game to get the manager for</param>
    /// <returns>The manager</returns>
    public static T GetManager<T>(this Games game, GameType? type = null)
        where T : GameManager
    {
        if (type == null)
        {
            if (typeof(T) == typeof(GameManager_Win32))
                type = GameType.Win32;

            else if (typeof(T) == typeof(GameManager_Steam))
                type = GameType.Steam;

            else if (typeof(T) == typeof(GameManager_WinStore))
                type = GameType.WinStore;

            else if (typeof(T) == typeof(GameManager_DOSBox))
                type = GameType.DosBox;

            else if (typeof(T) == typeof(GameManager_EducationalDOSBox))
                type = GameType.EducationalDosBox;

            else
                throw new Exception("The provided game manager type is not valid");
        }

        var g = Services.App.GamesManager;
        return g.CreateCachedInstance<T>(g.GameManagers[game][type.Value]);
    }

    /// <summary>
    /// Gets the game info for the specified game
    /// </summary>
    /// <param name="game">The game to get the info for</param>
    /// <returns>The info</returns>
    public static GameInfo GetGameInfo(this Games game) => GetGameInfo<GameInfo>(game);

    /// <summary>
    /// Gets the game info for the specified game
    /// </summary>
    /// <param name="game">The game to get the info for</param>
    /// <returns>The info</returns>
    public static T GetGameInfo<T>(this Games game)
        where T : GameInfo
    {
        var g = Services.App.GamesManager;
        return g.CreateCachedInstance<T>(g.GameInfos[game]);
    }

    // TODO-14: Remove once no longer needed
    public static GameInstallation GetInstallation(this Games game) => Services.Data.Game_GameInstallations.First(x => x.Game == game);
}