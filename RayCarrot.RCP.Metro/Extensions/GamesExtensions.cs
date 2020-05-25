using RayCarrot.Common;
using RayCarrot.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Games"/>
    /// </summary>
    public static class GamesExtensions
    {
        /// <summary>
        /// Determines if the specified game has been added to the program
        /// </summary>
        /// <param name="game">The game to check if it's added</param>
        /// <returns>True if the game has been added, otherwise false</returns>
        public static bool IsAdded(this Games game)
        {
            return RCPServices.Data.Games.ContainsKey(game);
        }

        /// <summary>
        /// Gets the installer items for the specified game
        /// </summary>
        /// <param name="game">The game to get the installer items for</param>
        /// <param name="outputPath">The output path for the installation</param>
        /// <returns>The installer items</returns>
        public static List<RayGameInstallItem> GetInstallerItems(this Games game, FileSystemPath outputPath)
        {
            // Create the result
            var result = new List<RayGameInstallItem>();

            // Attempt to get the text file containing the items
            if (!(InstallerGames.ResourceManager.GetObject($"{game}") is string file))
                throw new Exception("Installer item not found");

            // Create a string reader
            using StringReader reader = new StringReader(file);

            // Keep track of the current line
            string line;

            // Enumerate each line
            while ((line = reader.ReadLine()) != null)
            {
                // Check if the item is optional, in which case it has a blank space before the path
                bool optional = line.StartsWith(" ");

                // Remove the blank space if optional
                if (optional)
                    line = line.Substring(1);

                // Add the item
                result.Add(new RayGameInstallItem(line, outputPath + line, optional));
            }

            // Return the items
            return result;
        }

        /// <summary>
        /// Gets the install directory for the game or an empty path if not found or if it doesn't exist
        /// </summary>
        /// <param name="game">The game to get the install directory for</param>
        /// <param name="throwIfNotFound">Indicates if an exception should be thrown if the directory is not found</param>
        /// <returns>The install directory or an empty path if not found or if it doesn't exist</returns>
        public static FileSystemPath GetInstallDir(this Games game, bool throwIfNotFound = true)
        {
            // Get the game data
            var data = RCPServices.Data.Games.TryGetValue(game);

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
            var data = RCPServices.Data.Games.TryGetValue(game);

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
            var data = RCPServices.Data.Games.TryGetValue(game);

            // Make sure it's not null
            if (data == null)
                throw new Exception($"The data for the requested game '{game}' could not be found");

            // Return the type
            return data.LaunchMode;
        }

        /// <summary>
        /// Gets the game manager for the specified game with the current type
        /// </summary>
        /// <param name="game">The game to get the manager for</param>
        /// <returns>The manager</returns>
        public static RCPGameManager GetManager(this Games game)
        {
            return RCPServices.App.GamesManager.GameManagers[game][game.GetGameType()].CreateInstance<RCPGameManager>();
        }

        /// <summary>
        /// Gets the available game managers for the specified game
        /// </summary>
        /// <param name="game">The game to get the managers for</param>
        /// <returns>The managers</returns>
        public static IEnumerable<RCPGameManager> GetManagers(this Games game)
        {
            return RCPServices.App.GamesManager.GameManagers[game].Values.Select(managerType => managerType.CreateInstance<RCPGameManager>());
        }

        /// <summary>
        /// Gets the game manager for the specified game
        /// </summary>
        /// <param name="game">The game to get the manager for</param>
        /// <param name="type">The type of game to get the manager for</param>
        /// <returns>The manager</returns>
        public static RCPGameManager GetManager(this Games game, GameType type)
        {
            return RCPServices.App.GamesManager.GameManagers[game][type].CreateInstance<RCPGameManager>();
        }

        /// <summary>
        /// Gets the game manager of the specified type for the specified game
        /// </summary>
        /// <typeparam name="T">The type of manager to get</typeparam>
        /// <param name="game">The game to get the manager for</param>
        /// <param name="type">The type of game to get the manager for</param>
        /// <returns>The manager</returns>
        public static T GetManager<T>(this Games game, GameType? type = null)
            where T : RCPGameManager
        {
            if (type == null)
            {
                if (typeof(T) == typeof(RCPWin32Game))
                    type = GameType.Win32;

                else if (typeof(T) == typeof(RCPSteamGame))
                    type = GameType.Steam;

                else if (typeof(T) == typeof(RCPWinStoreGame))
                    type = GameType.WinStore;

                else if (typeof(T) == typeof(RCPDOSBoxGame))
                    type = GameType.DosBox;

                else if (typeof(T) == typeof(RCPEducationalDOSBoxGame))
                    type = GameType.EducationalDosBox;

                else
                    throw new Exception("The provided game manager type is not valid");
            }

            return RCPServices.App.GamesManager.GameManagers[game][type.Value].CreateInstance<T>();
        }

        /// <summary>
        /// Gets the game info for the specified game
        /// </summary>
        /// <param name="game">The game to get the info for</param>
        /// <returns>The info</returns>
        public static RCPGameInfo GetGameInfo(this Games game)
        {
            return RCPServices.App.GamesManager.GameInfos[game].CreateInstance<RCPGameInfo>();
        }

        /// <summary>
        /// Gets the game info for the specified game
        /// </summary>
        /// <param name="game">The game to get the info for</param>
        /// <returns>The info</returns>
        public static T GetGameInfo<T>(this Games game)
            where T : RCPGameInfo
        {
            return RCPServices.App.GamesManager.GameInfos[game].CreateInstance<T>();
        }
    }
}