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
            try
            {
                // Declare variables
                List<Games> gamesToFind = games.ToList();
                List<GameFinderResult> foundGames = new List<GameFinderResult>();

                RCFCore.Logger?.LogInformationSource($"The game finder is searching for the following games: {gamesToFind.JoinItems(", ")}");

                // Get the game finder items
                var gameFinders = gamesToFind.
                    SelectMany(x => x.GetManagers().Where(z => z.GameFinderItem != null).Select(y => new GameFinderItemContainer(x, y.Type, y.GameFinderItem))).
                    ToArray();

                RCFCore.Logger?.LogTraceSource($"{gameFinders.Length} game finders were found");

                // Split finders into groups
                var ubiIniGameFinders = gameFinders.Where(x => x.FinderItem.UbiIniSectionName != null).ToArray();
                var regUninstallGameFinders = gameFinders.Where(x => x.FinderItem.PossibleWin32Names?.Any() == true).ToList();
                var programShortcutGameFinders = gameFinders.Where(x => x.FinderItem.ShortcutName != null).ToList();
                var steamGameFinders = gameFinders.Where(x => x.FinderItem.SteamID != null).ToList();
                var customGameFinders = gameFinders.Where(x => x.FinderItem.CustomFinderAction != null).ToArray();

                // Helper method for adding a found game
                bool AddGame(GameFinderItemContainer game, FileSystemPath installDir)
                {
                    RCFCore.Logger?.LogInformationSource($"An install directory was found for {game.Game}");

                    // Make sure the game hasn't already been found
                    if (foundGames.Any(x => x.Game == game.Game))
                    {
                        RCFCore.Logger?.LogInformationSource($"{game.Game} could not be added. The game has already been found.");
                        return false;
                    }

                    // Make sure the install directory exists
                    if (!installDir.DirectoryExists)
                    {
                        RCFCore.Logger?.LogInformationSource($"{game.Game} could not be added. The install directory does not exist.");
                        return false;
                    }

                    // If available, run custom verification
                    if (game.FinderItem.VerifyInstallDirectory != null)
                    {
                        var result = game.FinderItem.VerifyInstallDirectory?.Invoke(installDir);

                        if (result == null)
                        {
                            RCFCore.Logger?.LogInformationSource($"{game.Game} could not be added. The optional verification returned null.");
                            return false;
                        }

                        installDir = result.Value;
                    }

                    // Make sure that the default file is found
                    if (!(installDir + game.Game.GetGameInfo().DefaultFileName).FileExists)
                    {
                        RCFCore.Logger?.LogInformationSource($"{game.Game} could not be added. The game default file was not found.");
                        return false;
                    }

                    // Add the game to found games
                    foundGames.Add(new GameFinderResult(game.Game, installDir, game.GameType));

                    // Remove from games to find
                    gamesToFind.Remove(game.Game);

                    RCFCore.Logger?.LogInformationSource($"The game {game.Game} was found");

                    return true;
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
                        {
                            // Get the sections and the directory for each one
                            iniLocations = GetUbiIniData();
                            RCFCore.Logger?.LogInformationSource("The ubi.ini file data was parsed for the game finder");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Reading ubi.ini file for game finder");
                    }

                    // If we retrieved ini data...
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
                    else
                    {
                        RCFCore.Logger?.LogInformationSource("The ubi.ini file data was null");
                    }
                }

                // Search Registry uninstall programs
                if ((regUninstallGameFinders.Any() || steamGameFinders.Any()) && gamesToFind.Any())
                {
                    RCFCore.Logger?.LogInformationSource("The Registry uninstall programs are being searched...");

                    try
                    {
                        // Enumerate each program
                        foreach (var program in EnumerateRegistryUninstallPrograms())
                        {
                            // Find all matches
                            IEnumerable<GameFinderItemContainer> gameMatches;

                            // Check if the program has a Steam ID associated with it
                            var isSteamGame = !program.SteamID.IsNullOrWhiteSpace();

                            // Attempt to find matching game
                            if (isSteamGame)
                                gameMatches = steamGameFinders.Where(x => x.FinderItem.SteamID == program.SteamID);
                            else
                                gameMatches = regUninstallGameFinders.Where(x => program.DisplayName.Equals(StringComparison.CurrentCultureIgnoreCase, x.FinderItem.PossibleWin32Names)).ToArray();

                            // Handle each game match
                            foreach (var gameMatch in gameMatches)
                            {
                                // Get the install location
                                var location = program.InstallLocation.
                                    // Replace the separator character as Uplay games use the wrong one
                                    Replace("/", @"\");

                                // Add the game
                                var added = AddGame(gameMatch, location);

                                // Remove if added
                                if (added)
                                {
                                    if (isSteamGame)
                                        steamGameFinders.Remove(gameMatch);
                                    else
                                        regUninstallGameFinders.Remove(gameMatch);
                                }
                            }

                            // Break if no more games needed to be found
                            if (!regUninstallGameFinders.Any() && !steamGameFinders.Any())
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Searching Registry uninstall programs for game finder");
                    }
                }

                // Search Win32 shortcuts
                if (programShortcutGameFinders.Any() && gamesToFind.Any())
                {
                    RCFCore.Logger?.LogInformationSource("The program shortcuts are being searched...");

                    try
                    {
                        // Enumerate each program
                        foreach (var shortcut in EnumerateProgramShortcuts())
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
                                var added = AddGame(gameMatch, targetDir);

                                // Remove if added
                                if (added)
                                    programShortcutGameFinders.Remove(gameMatch);
                            }

                            // Break if no more games needed to be found
                            if (!programShortcutGameFinders.Any())
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Searching program shortcuts for game finder");
                    }
                }

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

                RCFCore.Logger?.LogInformationSource($"The game finder found {foundGames.Count} games");

                // Return the found games
                return foundGames.AsReadOnly();
            }
            catch (Exception ex)
            {
                ex.HandleError("Game finder", games);

                throw;
            }
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
            // Get items from user start menu
            foreach (string file in EnumerateShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), SearchOption.AllDirectories))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The user start menu programs were retrieved for the game finder");

            // Get items from common start menu
            foreach (string file in EnumerateShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), SearchOption.AllDirectories))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The common start menu programs were retrieved for the game finder");

            // Get items from user desktop
            foreach (string file in EnumerateShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The user desktop shortcuts were retrieved for the game finder");

            // Get items from common desktop
            foreach (string file in EnumerateShortcuts(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)))
            {
                // Yield return the item
                yield return file;
            }

            RCFCore.Logger?.LogTraceSource("The common desktop shortcuts were retrieved for the game finder");
        }

        /// <summary>
        /// Enumerates the program shortcuts from the specified directory
        /// </summary>
        /// <param name="directory">The directory to get the shortcuts from</param>
        /// <param name="searchOption">The search option to use</param>
        /// <returns>The program shortcut paths</returns>
        protected virtual IEnumerable<string> EnumerateShortcuts(FileSystemPath directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                // Get items from specified directory
                return Directory.EnumerateFiles(directory, "*.lnk", searchOption);
            }
            catch (Exception ex)
            {
                ex.HandleError("Enumerating shortcuts for game finder", directory);
                
                // Return an empty array to enumerate
                return new string[0];
            }
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