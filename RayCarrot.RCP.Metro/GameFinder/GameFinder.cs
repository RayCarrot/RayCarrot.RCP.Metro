using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniParser;
using Microsoft.Win32;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game finder, used to find installed games
    /// </summary>
    public class GameFinder
    {
        #region Public Methods

        /// <summary>
        /// Attempts to find the specified games, returning the found games and their install locations
        /// </summary>
        /// <param name="games">The games to search for</param>
        /// <returns>The found games and their install locations</returns>
        public IReadOnlyList<GameFinderResult> FindGames(IEnumerable<Games> games)
        {
            // TODO: Add more logging

            // Declare variables
            List<Games> gamesToFind = games.ToList();
            List<GameFinderResult> foundGames = new List<GameFinderResult>();

            // Get the game finder items
            var gameFinders = gamesToFind.SelectMany(x => x.GetManagers().Where(z => z.GameFinderItem != null).Select(y => new GameFinderItemContainer(x, y.Type, y.GameFinderItem))).ToArray();

            // Split finders into groups
            var ubiIniGameFinders = gameFinders.Where(x => x.FinderItem.UbiIniSectionName != null).ToArray();
            var regUninstallGameFinders = gameFinders.Where(x => x.FinderItem.PossibleWin32Names?.Any() == true).ToArray();
            var programShortcutGameFinders = gameFinders.Where(x => x.FinderItem.ShortcutName != null).ToArray();
            var steamGameFinders = gameFinders.Where(x => x.FinderItem.SteamID != null).ToArray();
            var customGameFinders = gameFinders.Where(x => x.FinderItem.CustomFinderAction != null).ToArray();

            // Helper method for adding a found game
            void AddGame(GameFinderItemContainer game, FileSystemPath installDir)
            {
                // Make sure the game hasn't already been found
                if (foundGames.Any(x => x.Game == game.Game))
                {
                    // TODO: Log
                    return;
                }

                // Make sure the install directory exists
                if (!installDir.DirectoryExists)
                {
                    // TODO: Log
                    return;
                }

                // If available, run custom verification
                if (game.FinderItem.VerifyInstallDirectory != null)
                {
                    var result = game.FinderItem.VerifyInstallDirectory?.Invoke(installDir);

                    if (result == null)
                    {
                        // TODO: Log
                        return;
                    }

                    installDir = result.Value;
                }

                // Make sure that the default file is found
                if (!(installDir + game.Game.GetGameInfo().DefaultFileName).FileExists)
                {
                    // TODO: Log
                    return;
                }

                // Add the game to found games
                foundGames.Add(new GameFinderResult(game.Game, installDir, game.GameType));

                // Remove from games to find
                gamesToFind.Remove(game.Game);
            }

            // Search the ubi.ini file
            if (ubiIniGameFinders.Any() && gamesToFind.Any())
            {
                Dictionary<string, string> iniLocations = null;

                RCFCore.Logger?.LogInformationSource("The game finder has ubi.ini finder");

                try
                {
                    // Make sure the file exists
                    if (CommonPaths.UbiIniPath1.FileExists)
                        // Get the sections and the directory for each one
                        iniLocations = GetUbiIniData();
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Reading ubi.ini file for game finder");
                }

                RCFCore.Logger?.LogInformationSource("The ubi.ini file data was parsed for the game finder");

                if (iniLocations != null)
                {
                    RCFCore.Logger?.LogInformationSource("The ini sections are being searched...");

                    // Enumerate each game finder item
                    foreach (var game in ubiIniGameFinders)
                    {
                        // Attempt to get the install location
                        var location = iniLocations.TryGetValue(game.FinderItem.UbiIniSectionName);

                        // Make sure we got a location
                        if (location.IsNullOrWhiteSpace())
                            continue;

                        // Add the game
                        AddGame(game, location);
                    }
                }
            }

            // Search Registry uninstall programs
            if ((regUninstallGameFinders.Any() || steamGameFinders.Any()) && gamesToFind.Any())
            {
                // Get the program iterator
                var regUninstallPrograms = EnumerateRegistryUninstallPrograms();

                RCFCore.Logger?.LogInformationSource("The Registry uninstall programs are being searched...");

                // TODO: Try/catch
                // Enumerate each program
                foreach (var program in regUninstallPrograms)
                {
                    // Find all matches
                    IEnumerable<GameFinderItemContainer> gameMatches;

                    // Check if the program has a Steam ID associated with it
                    if (!program.SteamID.IsNullOrWhiteSpace())
                        // Attempt to find matching game
                        gameMatches = steamGameFinders.Where(x => x.FinderItem.SteamID == program.SteamID);
                    else
                        // Attempt to find matching game name
                        gameMatches = regUninstallGameFinders.Where(x => program.DisplayName.Equals(StringComparison.CurrentCultureIgnoreCase, x.FinderItem.PossibleWin32Names));

                    // Handle each game match
                    foreach (var gameMatch in gameMatches)
                    {
                        // Get the install location
                        var location = program.InstallLocation.
                            // Replace the separator character as Uplay games use the wrong one
                            Replace("/", @"\");

                        // Add the game
                        AddGame(gameMatch, location);

                        // TODO: Break if no more games needed to be found
                    }
                }
            }

            // Search Win32 shortcuts
            if (programShortcutGameFinders.Any() && gamesToFind.Any())
            {
                // Get the shortcut iterator
                var programShortcuts = EnumerateProgramShortcuts();

                RCFCore.Logger?.LogInformationSource("The program shortcuts are being searched...");

                // TODO: Try/catch
                // Enumerate each program
                foreach (var shortcut in programShortcuts)
                {
                    // Get the file name
                    var file = Path.GetFileNameWithoutExtension(shortcut);

                    // Make sure we got a file
                    if (file == null)
                        continue;

                    // Handle each game match
                    foreach (var gameMatch in programShortcutGameFinders.Where(x => file.IndexOf(x.FinderItem.ShortcutName, StringComparison.CurrentCultureIgnoreCase) > -1))
                    {
                        FileSystemPath targetDir;

                        try
                        {
                            // Attempt to get the shortcut target path
                            targetDir = WindowsHelpers.GetShortCutTarget(shortcut).Parent;
                        }
                        catch (Exception ex)
                        {
                            ex.HandleUnexpected("Getting start menu item shortcut target for game finder", shortcut);
                            continue;
                        }

                        // Add the game
                        AddGame(gameMatch, targetDir);
                    }

                    // TODO: Break if no more games needed to be found
                }
            }

            // TODO: Search Program Files?

            // TODO: Hard-code DOSBox search?
            //// Attempt to find DOSBox if not added
            //if (!File.Exists(RCFRCP.Data.DosBoxPath))
            //{
            //    var actions = new Func<GameFinderActionResult>[]
            //    {
            //        // Uninstall
            //        () => CheckUninstall("DosBox", "Dos Box"),

            //        // Start menu
            //        () => CheckShortcuts("DosBox", "DOSBox.exe"),
            //    };

            //    // Run every check action until one is successful
            //    foreach (var action in actions)
            //    {
            //        // Stop running the check actions if the file has been found
            //        if (File.Exists(Data.DosBoxPath))
            //            break;

            //        try
            //        {
            //            // Get the result from the action
            //            var dosBoxCheckResult = action();

            //            var filePath = dosBoxCheckResult.Path + "DOSBox.exe";

            //            // Check if the file exists
            //            if (!filePath.FileExists)
            //                continue;

            //            RCFCore.Logger?.LogTraceSource($"The DosBox executable was found from the game checker with the source {dosBoxCheckResult.Source}");

            //            Data.DosBoxPath = filePath;

            //            break;
            //        }
            //        catch (Exception ex)
            //        {
            //            ex.HandleUnexpected("DosBox check action", action);
            //        }
            //    }
            //}

            // Run custom finders
            foreach (var game in customGameFinders)
            {
                // Make sure the game hasn't already been found
                if (foundGames.Any(x => x.Game == game.Game))
                    continue;

                // Run the custom action and get the result
                var result = game.FinderItem.CustomFinderAction();

                // Make sure we got a result
                if (result == null)
                    continue;

                // Add the game
                AddGame(game, result.Value);
            }

            return foundGames.AsReadOnly();
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Gets the data from the ubi.ini file
        /// </summary>
        /// <returns>Á dictionary of the sections and their the install location value</returns>
        protected virtual Dictionary<string, string> GetUbiIniData()
        {
            // Create the ini data parser
            return new FileIniDataParser(new UbiIniDataParser()).
                // Read the primary ubi.ini file
                ReadFile(CommonPaths.UbiIniPath1).
                // Get the sections
                Sections.
                // Create a dictionary
                ToDictionary(x => x.SectionName, x => x.Keys.GetKeyData("Directory")?.Value);
        }

        /// <summary>
        /// Enumerates the program shortcuts from common locations such as the Start Menu and Desktop
        /// </summary>
        /// <returns>The program shortcut paths</returns>
        protected virtual IEnumerable<string> EnumerateProgramShortcuts()
        {
            // TODO: Make sure each location has read access first?

            // Get items from user start menu
            foreach (string file in Directory.EnumerateFiles(
                // Get start menu shortcuts
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                // Filter to shortcuts only
                "*.lnk",
                // Search all directories
                SearchOption.AllDirectories))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The user start menu programs were retrieved for the game finder");

            // Get items from user start menu
            foreach (string file in Directory.EnumerateFiles(
                // Get common start menu shortcuts
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                // Filter to shortcuts only
                "*.lnk",
                // Search all directories
                SearchOption.AllDirectories))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The common start menu programs were retrieved for the game finder");

            // Get items from user start menu
            foreach (string file in Directory.EnumerateFiles(
                // Get desktop shortcuts
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                // Filter to shortcuts only
                "*.lnk",
                // Search top directory only
                SearchOption.TopDirectoryOnly))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The user desktop shortcuts were retrieved for the game finder");

            // Get items from user start menu
            foreach (string file in Directory.EnumerateFiles(
                // Get common desktop shortcuts
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                // Filter to shortcuts only
                "*.lnk",
                // Search top directory only
                SearchOption.TopDirectoryOnly))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The common desktop shortcuts were retrieved for the game finder");
        }

        /// <summary>
        /// Enumerates the programs from the Registry uninstall key
        /// </summary>
        /// <returns>The programs</returns>
        protected virtual IEnumerable<InstalledProgram> EnumerateRegistryUninstallPrograms()
        {
            RCFCore.Logger?.LogInformationSource("Getting installed programs from the Registry");

            // Get 64-bit location if on 64-bit system
            var keys = Environment.Is64BitOperatingSystem
                ? new RegistryKey[]
                {
                    RCFWinReg.RegistryManager.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms,
                        RegistryView.Registry32),
                    RCFWinReg.RegistryManager.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms,
                        RegistryView.Registry64)
                }
                : new RegistryKey[]
                {
                    RCFWinReg.RegistryManager.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms,
                        RegistryView.Registry32),
                };

            // Enumerate the uninstall keys
            foreach (var key in keys)
            {
                // Dispose key when done
                using RegistryKey registryKey = key;

                // Enumerate the sub keys
                foreach (string subKeyName in registryKey.GetSubKeyNames())
                {
                    // Make sure it's not a Windows update
                    if (subKeyName.StartsWith("KB") && subKeyName.Length == 8)
                        continue;

                    // Open the sub key
                    using RegistryKey subKey = registryKey.OpenSubKey(subKeyName);

                    // Make sure the key is not null    
                    if (subKey == null)
                        continue;

                    // Make sure it is not a system component
                    if (subKey.GetValue("SystemComponent") as int? == 1)
                        continue;

                    if (subKey.GetValue("WindowsInstaller") as int? == 1)
                        continue;

                    // Make sure it has an uninstall string
                    if (!subKey.HasValue("UninstallString"))
                        continue;

                    if (subKey.HasValue("ParentKeyName"))
                        continue;

                    // Make sure it has a display name
                    if (!(subKey.GetValue("DisplayName") is string dn))
                        continue;

                    // Make sure it has an install location
                    if (!(subKey.GetValue("InstallLocation") is string dir))
                        continue;

                    yield return new InstalledProgram(dn, subKey.Name, dir);
                }
            }
        }

        #endregion

        #region Protected Classes

        /// <summary>
        /// An installed program
        /// </summary>
        protected class InstalledProgram
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="displayName">The program display name</param>
            /// <param name="registryKeyPath">The program uninstall Registry path</param>
            /// <param name="installLocation">The program install location, if any</param>
            public InstalledProgram(string displayName, string registryKeyPath, string installLocation)
            {
                DisplayName = displayName;
                InstallLocation = installLocation;

                const string steamAppName = "Steam App";

                // Check if it's a Steam app
                if (registryKeyPath.Contains(steamAppName))
                    // Get the Steam ID
                    SteamID = registryKeyPath.Substring(registryKeyPath.IndexOf(steamAppName, StringComparison.Ordinal) + steamAppName.Length).Trim();
            }

            /// <summary>
            /// The program display name
            /// </summary>
            public string DisplayName { get; }

            /// <summary>
            /// The program install location, if any
            /// </summary>
            public string InstallLocation { get; }

            /// <summary>
            /// Indicates if the program is a Steam game
            /// </summary>
            public string SteamID { get; }
        }

        /// <summary>
        /// An container for a <see cref="GameFinderItem"/> with the game and type
        /// </summary>
        protected class GameFinderItemContainer
        {
            #region Constructors

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="game">The game to find</param>
            /// <param name="gameType">The game type</param>
            /// <param name="finderItem">The game finder item</param>
            public GameFinderItemContainer(Games game, GameType gameType, GameFinderItem finderItem)
            {
                Game = game;
                GameType = gameType;
                FinderItem = finderItem;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// The game to find
            /// </summary>
            public Games Game { get; }

            /// <summary>
            /// The game type
            /// </summary>
            public GameType GameType { get; }

            /// <summary>
            /// The game finder item
            /// </summary>
            public GameFinderItem FinderItem { get; }

            #endregion
        }

        #endregion
    }
}