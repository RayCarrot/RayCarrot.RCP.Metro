using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Games"/>
    /// </summary>
    public static class GamesExtensions
    {
        #region Helpers

        /// <summary>
        /// Determines if the specified game has been added to the program
        /// </summary>
        /// <param name="game">The game to check if it's added</param>
        /// <returns>True if the game has been added, otherwise false</returns>
        public static bool IsAdded(this Games game)
        {
            return RCFRCP.Data.Games.ContainsKey(game);
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

            // Attempt to get the text file
            if (!(InstallerGames.ResourceManager.GetObject($"{game}") is string file))
                throw new Exception("Installer item not found");

            using (StringReader reader = new StringReader(file))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    // Check if the item is optional, in which case
                    // it has a blank space before the path
                    bool optional = line.StartsWith(" ");

                    // Remove the blank space if optional
                    if (optional)
                        line = line.Substring(1);

                    result.Add(new RayGameInstallItem(line, outputPath + line, optional));
                }
            }

            return result;
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets the game manager for the specified game with the current type
        /// </summary>
        /// <param name="game">The game to get the manager for</param>
        /// <returns>The manager</returns>
        public static RCPGameManager GetManager(this Games game)
        {
            return RCFRCP.App.GameManagers[game][game.GetGameType()].CreateInstance<RCPGameManager>();
        }

        /// <summary>
        /// Gets the available game managers for the specified game
        /// </summary>
        /// <param name="game">The game to get the managers for</param>
        /// <returns>The managers</returns>
        public static IEnumerable<RCPGameManager> GetManagers(this Games game)
        {
            return RCFRCP.App.GameManagers[game].Values.Select(managerType => managerType.CreateInstance<RCPGameManager>());
        }

        /// <summary>
        /// Gets the game manager for the specified game
        /// </summary>
        /// <param name="game">The game to get the manager for</param>
        /// <param name="type">The type of game to get the manager for</param>
        /// <returns>The manager</returns>
        public static RCPGameManager GetManager(this Games game, GameType type)
        {
            return RCFRCP.App.GameManagers[game][type].CreateInstance<RCPGameManager>();
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

            return RCFRCP.App.GameManagers[game][type.Value].CreateInstance<T>();
        }

        /// <summary>
        /// Gets the game info for the specified game
        /// </summary>
        /// <param name="game">The game to get the info for</param>
        /// <returns>The info</returns>
        public static RCPGameInfo GetGameInfo(this Games game)
        {
            return RCFRCP.App.GameInfos[game].CreateInstance<RCPGameInfo>();
        }

        /// <summary>
        /// Gets the game info for the specified game
        /// </summary>
        /// <param name="game">The game to get the info for</param>
        /// <returns>The info</returns>
        public static T GetGameInfo<T>(this Games game)
            where T : RCPGameInfo
        {
            return RCFRCP.App.GameInfos[game].CreateInstance<T>();
        }

        #endregion
    }
}