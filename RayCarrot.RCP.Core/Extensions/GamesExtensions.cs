using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Extension methods for <see cref="Games"/>
    /// </summary>
    public static class GamesExtensions
    {
        /// <summary>
        /// Gets the install directory for the game or an empty path if not found or if it doesn't exist
        /// </summary>
        /// <param name="game">The game to get the install directory for</param>
        /// <param name="throwIfNotFound">Indicates if an exception should be thrown if the directory is not found</param>
        /// <returns>The install directory or an empty path if not found or if it doesn't exist</returns>
        public static FileSystemPath GetInstallDir(this Games game, bool throwIfNotFound = true)
        {
            // Get the game data
            var data = RCFRCPC.API.GetGameData(game);

            // Make sure it's not null
            if (data == null)
            {
                if (throwIfNotFound)
                    throw new Exception($"The data for the requested game '{game}' could not be found");
                else
                    return FileSystemPath.EmptyPath;
            }

            // Get the path
            var installDir = data.InstallDirectory;

            // Return the path
            return installDir;
        }

        /// <summary>
        /// Gets the installed game type for the game
        /// </summary>
        /// <param name="game">The game to get the installed game type for</param>
        /// <returns>The game type</returns>
        public static GameType GetGameType(this Games game)
        {
            // Get the game data
            var data = RCFRCPC.API.GetGameData(game);

            // Make sure it's not null
            if (data == null)
                throw new Exception($"The data for the requested game '{game}' could not be found");

            // Return the type
            return data.GameType;
        }

        /// <summary>
        /// Gets the saved launch mode for the game
        /// </summary>
        /// <param name="game">The game to get the saved launch mode type for</param>
        /// <returns>The saved launch mode</returns>
        public static GameLaunchMode GetLaunchMode(this Games game)
        {
            // Get the game data
            var data = RCFRCPC.API.GetGameData(game);

            // Make sure it's not null
            if (data == null)
                throw new Exception($"The data for the requested game '{game}' could not be found");

            // Return the type
            return data.LaunchMode;
        }
    }
}