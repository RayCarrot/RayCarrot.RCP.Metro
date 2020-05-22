using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using Microsoft.Win32;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman.UbiIni;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game finder, used to find installed games
    /// </summary>
    public class GameFinder
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="games">The games to search for</param>
        /// <param name="finderItems">Other finder items to search for</param>
        public GameFinder(IEnumerable<Games> games, IEnumerable<FinderItem> finderItems)
        {
            // Set properties
            GamesToFind = games.ToList();
            FinderItems = finderItems?.ToArray() ?? new FinderItem[0];
            FoundFinderItems = new List<FinderItem>();
            Results = new List<BaseFinderResult>();
            HasRun = false;

            RL.Logger?.LogInformationSource($"The game finder has been created to search for the following games: {GamesToFind.JoinItems(", ")}");

            // Get the game finder items
            GameFinderItems = GamesToFind.
                SelectMany(x => x.GetManagers().Where(z => z.GameFinderItem != null).Select(y => new GameFinderItemContainer(x, y.Type, y.GameFinderItem))).
                ToArray();

            RL.Logger?.LogTraceSource($"{GameFinderItems.Length} game finders were found");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to find the specified games, returning the found games and their install locations. This method can only be called once per class instance.
        /// </summary>
        /// <returns>The found games and their install locations</returns>
        public async Task<IReadOnlyList<BaseFinderResult>> FindGamesAsync()
        {
            if (HasRun)
                throw new Exception("The FindGames method can only be called once per instance");

            HasRun = true;

            try
            {
                // Split finders into groups
                var ubiIniGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.Game) && x.FinderItem.UbiIniSectionName != null).ToArray();

                // Search the ubi.ini file
                if (ubiIniGameFinders.Any() && GamesToFind.Any())
                {
                    IDictionary<string, string> iniLocations = null;

                    RL.Logger?.LogInformationSource("The game finder has ubi.ini finder");

                    try
                    {
                        // Make sure the file exists
                        if (CommonPaths.UbiIniPath1.FileExists)
                        {
                            // Get the sections and the directory for each one
                            iniLocations = GetUbiIniData();
                            RL.Logger?.LogInformationSource("The ubi.ini file data was parsed for the game finder");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Reading ubi.ini file for game finder");
                    }

                    // If we retrieved ini data, search it
                    if (iniLocations != null)
                        await SearchIniDataAsync(iniLocations, ubiIniGameFinders);
                    else
                        RL.Logger?.LogInformationSource("The ubi.ini file data was null");
                }

                // Split finders into groups
                var regUninstallGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.Game) && x.FinderItem.PossibleWin32Names?.Any() == true).ToList();
                var regUninstallFinders = FinderItems.Where(x => !FoundFinderItems.Contains(x) && x.PossibleWin32Names?.Any() == true).ToList();
                var steamGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.Game) && x.FinderItem.SteamID != null).ToList();

                // Search Registry uninstall programs
                if ((regUninstallGameFinders.Any() || steamGameFinders.Any() || regUninstallFinders.Any()) && GamesToFind.Any())
                {
                    RL.Logger?.LogInformationSource("The Registry uninstall programs are being searched...");

                    try
                    {
                        // Get the enumerator for installed programs
                        var installedPrograms = EnumerateRegistryUninstallPrograms();

                        // Search installed programs
                        await SearchRegistryUninstallAsync(installedPrograms, regUninstallGameFinders, steamGameFinders, regUninstallFinders);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Searching Registry uninstall programs for game finder");
                    }
                }

                // Split finders into groups
                var programShortcutGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.Game) && x.FinderItem.ShortcutName != null).ToList();
                var programShortcutFinders = FinderItems.Where(x => !FoundFinderItems.Contains(x) && x.ShortcutName != null).ToList();

                // Search Win32 shortcuts
                if ((programShortcutGameFinders.Any() || programShortcutFinders.Any()) && GamesToFind.Any())
                {
                    RL.Logger?.LogInformationSource("The program shortcuts are being searched...");

                    try
                    {
                        // Get the enumerator for the shortcuts
                        var shortcuts = EnumerateProgramShortcuts();

                        // Search the shortcuts
                        await SearchWin32ShortcutsAsync(shortcuts, programShortcutGameFinders, programShortcutFinders);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Searching program shortcuts for game finder");
                    }
                }

                // Run custom game finders
                foreach (var game in GameFinderItems.Where(x => !FoundGames.Contains(x.Game) && x.FinderItem.CustomFinderAction != null))
                {
                    // Run the custom action and get the result
                    var result = game.FinderItem.CustomFinderAction();

                    // Make sure we got a result
                    if (result == null)
                        continue;

                    // Add the game
                    await AddGameAsync(game, result.InstallDir, result.Parameter);
                }

                // Run custom finders
                foreach (var item in FinderItems.Where(x => !FoundFinderItems.Contains(x) && x.CustomFinderAction != null))
                {
                    // Run the custom action and get the result
                    var result = item.CustomFinderAction();

                    // Make sure we got a result
                    if (result == null)
                        continue;

                    // Add the game
                    AddItem(item, result.InstallDir, result.Parameter);
                }

                RL.Logger?.LogInformationSource($"The game finder found {Results.Count} games");

                // Return the found games
                return Results.AsReadOnly();
            }
            catch (Exception ex)
            {
                ex.HandleError("Game finder");
                throw;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The generic finder items
        /// </summary>
        protected FinderItem[] FinderItems { get; }

        /// <summary>
        /// The found finder items
        /// </summary>
        protected List<FinderItem> FoundFinderItems { get; }

        /// <summary>
        /// The game finder items
        /// </summary>
        protected GameFinderItemContainer[] GameFinderItems { get; }

        /// <summary>
        /// The list of games which are left to be found
        /// </summary>
        protected List<Games> GamesToFind { get; }

        /// <summary>
        /// Gets the games which have been found
        /// </summary>
        protected IEnumerable<Games> FoundGames => Results.OfType<GameFinderResult>().Select(x => x.Game);

        /// <summary>
        /// The list of game finder results
        /// </summary>
        protected List<BaseFinderResult> Results { get; }

        /// <summary>
        /// Indicates if the finder operation has run
        /// </summary>
        protected bool HasRun { get; private set; }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Searches the Win32 program shortcuts for matching game install directories
        /// </summary>
        /// <param name="shortcuts">The shortcut paths</param>
        /// <param name="programShortcutGameFinders">The shortcut game finders</param>
        /// <param name="programShortcutFinders">The shortcut finders</param>
        /// <returns>The task</returns>
        protected virtual async Task SearchWin32ShortcutsAsync(IEnumerable<string> shortcuts, List<GameFinderItemContainer> programShortcutGameFinders, List<FinderItem> programShortcutFinders)
        {
            // Enumerate each program
            foreach (var shortcut in shortcuts)
            {
                // Get the file name
                var file = Path.GetFileNameWithoutExtension(shortcut);

                // Make sure we got a file
                if (file == null)
                    continue;

                // Check matches towards other finder items
                foreach (var finderItem in programShortcutFinders.Where(x => file.IndexOf(x.ShortcutName, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray())
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

                    // Add the item
                    var added = AddItem(finderItem, targetDir);

                    // Remove if added
                    if (added)
                        programShortcutFinders.Remove(finderItem);
                }

                // Handle each game match
                foreach (var gameMatch in programShortcutGameFinders.Where(x => file.IndexOf(x.FinderItem.ShortcutName, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray())
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
                    var added = await AddGameAsync(gameMatch, targetDir);

                    // Remove if added
                    if (added)
                        programShortcutGameFinders.Remove(gameMatch);
                }

                // Break if no more games needed to be found
                if (!programShortcutGameFinders.Any() && !programShortcutFinders.Any())
                    break;
            }
        }

        /// <summary>
        /// Searches the Registry uninstall programs for matching game install directories
        /// </summary>
        /// <param name="installedPrograms">The installed programs found from the Registry</param>
        /// <param name="regUninstallGameFinders">The Registry uninstall game finders</param>
        /// <param name="steamGameFinders">The Steam game finders</param>
        /// <param name="regUninstallFinders">The Registry uninstall finders</param>
        /// <returns>The task</returns>
        protected virtual async Task SearchRegistryUninstallAsync(IEnumerable<InstalledProgram> installedPrograms, List<GameFinderItemContainer> regUninstallGameFinders, List<GameFinderItemContainer> steamGameFinders, List<FinderItem> regUninstallFinders)
        {
            // Enumerate each program
            foreach (var program in installedPrograms)
            {
                // Check matches towards other finder items
                foreach (var finderItem in regUninstallFinders.Where(x => program.DisplayName.Equals(StringComparison.CurrentCultureIgnoreCase, x.PossibleWin32Names)).ToArray())
                {
                    // Add the item
                    var added = AddItem(finderItem, program.InstallLocation);

                    // Remove if added
                    if (added)
                        regUninstallFinders.Remove(finderItem);
                }

                // Find all game matches
                IEnumerable<GameFinderItemContainer> gameMatches;

                // Check if the program has a Steam ID associated with it
                var isSteamGame = !program.SteamID.IsNullOrWhiteSpace();

                // Attempt to find matching game
                if (isSteamGame)
                    gameMatches = steamGameFinders.Where(x => x.FinderItem.SteamID == program.SteamID).ToArray();
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
                    var added = await AddGameAsync(gameMatch, location);

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
                if (!regUninstallGameFinders.Any() && !steamGameFinders.Any() && !regUninstallFinders.Any())
                    break;
            }
        }

        /// <summary>
        /// Searches the ini data for matching game install directories
        /// </summary>
        /// <param name="iniInstallDirData">A dictionary of the sections and their the install location value</param>
        /// <param name="finders">The game finders for this operation</param>
        /// <returns>The task</returns>
        protected virtual async Task SearchIniDataAsync(IDictionary<string, string> iniInstallDirData, GameFinderItemContainer[] finders)
        {
            RL.Logger?.LogInformationSource("The ini sections are being searched...");

            // Enumerate each game finder item
            foreach (var game in finders)
            {
                // Attempt to get the install location
                var location = iniInstallDirData.TryGetValue(game.FinderItem.UbiIniSectionName);

                // Make sure we got a location
                if (location.IsNullOrWhiteSpace())
                    continue;

                // Add the game
                await AddGameAsync(game, location);
            }
        }

        /// <summary>
        /// Gets the data from the ubi.ini file
        /// </summary>
        /// <returns>A dictionary of the sections and their the install location value</returns>
        protected virtual IDictionary<string, string> GetUbiIniData()
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
            foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.StartMenu.GetFolderPath(), SearchOption.AllDirectories))
            {
                // Yield return the item
                yield return file;
            }

            RL.Logger?.LogTraceSource("The user start menu programs were retrieved for the game finder");

            // Get items from common start menu
            foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.CommonStartMenu.GetFolderPath(), SearchOption.AllDirectories))
            {
                // Yield return the item
                yield return file;
            }

            RL.Logger?.LogTraceSource("The common start menu programs were retrieved for the game finder");

            // Get items from user desktop
            foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.DesktopDirectory.GetFolderPath()))
            {
                // Yield return the item
                yield return file;
            }

            RL.Logger?.LogTraceSource("The user desktop shortcuts were retrieved for the game finder");

            // Get items from common desktop
            foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.CommonDesktopDirectory.GetFolderPath()))
            {
                // Yield return the item
                yield return file;
            }

            RL.Logger?.LogTraceSource("The common desktop shortcuts were retrieved for the game finder");
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
                ex.HandleUnexpected("Enumerating shortcuts for game finder", directory);
                
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
            RL.Logger?.LogInformationSource("Getting installed programs from the Registry");

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

        /// <summary>
        /// Attempts to add a found <see cref="GameFinderItemContainer"/>
        /// </summary>
        /// <param name="game">The game item to add</param>
        /// <param name="installDir">The found install directory</param>
        /// <param name="parameter">Optional parameter</param>
        /// <returns>True if the game item was added, otherwise false</returns>
        protected virtual async Task<bool> AddGameAsync(GameFinderItemContainer game, FileSystemPath installDir, object parameter = null)
        {
            RL.Logger?.LogInformationSource($"An install directory was found for {game.Game}");

            // Make sure the game hasn't already been found
            if (FoundGames.Contains(game.Game))
            {
                RL.Logger?.LogWarningSource($"{game.Game} could not be added. The game has already been found.");
                return false;
            }

            // Make sure the install directory exists
            if (!installDir.DirectoryExists)
            {
                RL.Logger?.LogWarningSource($"{game.Game} could not be added. The install directory does not exist.");
                return false;
            }

            // If available, run custom verification
            if (game.FinderItem.VerifyInstallDirectory != null)
            {
                var result = game.FinderItem.VerifyInstallDirectory?.Invoke(installDir);

                if (result == null)
                {
                    RL.Logger?.LogInformationSource($"{game.Game} could not be added. The optional verification returned null.");
                    return false;
                }

                installDir = result.Value;
            }

            // Make sure that the game is valid
            if (!await game.Game.GetManager(game.GameType).IsValidAsync(installDir, parameter))
            {
                RL.Logger?.LogInformationSource($"{game.Game} could not be added. The game default file was not found.");
                return false;
            }

            // Add the game to found games
            Results.Add(new GameFinderResult(game.Game, installDir, game.GameType, game.FinderItem.FoundAction, parameter));

            // Remove from games to find
            GamesToFind.Remove(game.Game);

            RL.Logger?.LogInformationSource($"The game {game.Game} was found");

            return true;
        }

        /// <summary>
        /// Attempts to add a found <see cref="FinderItem"/>
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="installDir">The found install directory</param>
        /// <param name="parameter">Optional parameter</param>
        /// <returns>True if the item was added, otherwise false</returns>
        protected virtual bool AddItem(FinderItem item, FileSystemPath installDir, object parameter = null)
        {
            RL.Logger?.LogInformationSource($"An install directory was found for a finder item");

            // Make sure the install directory exists
            if (!installDir.DirectoryExists)
            {
                RL.Logger?.LogWarningSource($"The item could not be added. The install directory does not exist.");
                return false;
            }

            // If available, run custom verification
            if (item.VerifyInstallDirectory != null)
            {
                var result = item.VerifyInstallDirectory?.Invoke(installDir);

                if (result == null)
                {
                    RL.Logger?.LogInformationSource($"The item could not be added. The optional verification returned null.");
                    return false;
                }

                installDir = result.Value;
            }

            // Add the item
            FoundFinderItems.Add(item);
            Results.Add(new FinderResult(installDir, item.FoundAction, parameter, item.DisplayName));

            RL.Logger?.LogInformationSource($"A finder item was found");

            return true;
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