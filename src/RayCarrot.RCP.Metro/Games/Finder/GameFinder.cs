using System.IO;
using IniParser;
using Microsoft.Win32;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

// TODO: Rewrite this

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
    public GameFinder(IEnumerable<GameDescriptor> games, IEnumerable<GameFinder_GenericItem>? finderItems)
    {
        // Set properties
        GamesToFind = new HashSet<GameDescriptor>(games);
        FinderItems = finderItems?.ToArray() ?? Array.Empty<GameFinder_GenericItem>();
        FoundFinderItems = new List<GameFinder_GenericItem>();
        Results = new List<GameFinder_BaseResult>();
        HasRun = false;

        Logger.Info("The game finder has been created to search for the following games: {0}", GamesToFind.JoinItems(", "));

        // Get the game finder items
        GameFinderItems = GamesToFind.
            Select(x => new { GameDescriptor = x, FinderItem = x.GetGameFinderItem() }).
            Where(x => x.FinderItem != null).
            Select(x => new GameFinderItemContainer(x.GameDescriptor, x.FinderItem!)).
            ToArray();

        Logger.Trace("{0} game finders were found", GameFinderItems.Length);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// The generic finder items
    /// </summary>
    private GameFinder_GenericItem[] FinderItems { get; }

    /// <summary>
    /// The found finder items
    /// </summary>
    private List<GameFinder_GenericItem> FoundFinderItems { get; }

    /// <summary>
    /// The game finder items
    /// </summary>
    private GameFinderItemContainer[] GameFinderItems { get; }

    /// <summary>
    /// The list of games which are left to be found
    /// </summary>
    private HashSet<GameDescriptor> GamesToFind { get; }

    /// <summary>
    /// Gets the games which have been found
    /// </summary>
    private IEnumerable<GameDescriptor> FoundGames => Results.OfType<GameFinder_GameResult>().Select(x => x.GameDescriptor);

    /// <summary>
    /// The list of game finder results
    /// </summary>
    private List<GameFinder_BaseResult> Results { get; }

    /// <summary>
    /// Indicates if the finder operation has run
    /// </summary>
    private bool HasRun { get; set; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Searches the Win32 program shortcuts for matching game install directories
    /// </summary>
    /// <param name="shortcuts">The shortcut paths</param>
    /// <param name="programShortcutGameFinders">The shortcut game finders</param>
    /// <param name="programShortcutFinders">The shortcut finders</param>
    private void SearchWin32Shortcuts(IEnumerable<string> shortcuts, List<GameFinderItemContainer> programShortcutGameFinders, List<GameFinder_GenericItem> programShortcutFinders)
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
            foreach (var finderItem in programShortcutFinders.Where(x => file.IndexOf(x.ShortcutName!, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray())
            {
                FileSystemPath targetDir;

                try
                {
                    // Attempt to get the shortcut target path
                    targetDir = ((FileSystemPath)WindowsHelpers.GetShortCutTarget(shortcut)).Parent;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Getting start menu item shortcut target for game finder {0}", shortcut);
                    continue;
                }

                // Add the item
                var added = AddItem(finderItem, targetDir);

                // Remove if added
                if (added)
                    programShortcutFinders.Remove(finderItem);
            }

            // Handle each game match
            foreach (var gameMatch in programShortcutGameFinders.Where(x => file.IndexOf(x.FinderItem.ShortcutName!, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray())
            {
                FileSystemPath targetDir;

                try
                {
                    // Attempt to get the shortcut target path
                    targetDir = ((FileSystemPath)WindowsHelpers.GetShortCutTarget(shortcut)).Parent;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Getting start menu item shortcut target for game finder {0}", shortcut);
                    continue;
                }

                // Add the game
                bool added = AddGame(gameMatch, targetDir);

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
    private void SearchRegistryUninstall(IEnumerable<InstalledProgram> installedPrograms, List<GameFinderItemContainer> regUninstallGameFinders, List<GameFinderItemContainer> steamGameFinders, List<GameFinder_GenericItem> regUninstallFinders)
    {
        // Enumerate each program
        foreach (InstalledProgram program in installedPrograms)
        {
            // Check matches towards other finder items
            foreach (GameFinder_GenericItem finderItem in regUninstallFinders.Where(x => x.PossibleWin32Names!.Any(item => program.DisplayName.Equals(item, StringComparison.CurrentCultureIgnoreCase))).ToArray())
            {
                // Add the item
                bool added = AddItem(finderItem, program.InstallLocation);

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
                gameMatches = regUninstallGameFinders.Where(x => x.FinderItem.PossibleWin32Names!.Any(item => program.DisplayName.Equals(item, StringComparison.CurrentCultureIgnoreCase))).ToArray();

            // Handle each game match
            foreach (var gameMatch in gameMatches)
            {
                // Get the install location
                string location = program.InstallLocation.
                    // Replace the separator character as Uplay games use the wrong one
                    Replace("/", @"\");

                // Add the game
                bool added = AddGame(gameMatch, location);

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
    private void SearchIniData(IDictionary<string, string?> iniInstallDirData, GameFinderItemContainer[] finders)
    {
        Logger.Info("The ini sections are being searched...");

        // Enumerate each game finder item
        foreach (GameFinderItemContainer game in finders)
        {
            // Attempt to get the install location
            string? location = iniInstallDirData.TryGetValue(game.FinderItem.UbiIniSectionName!);

            // Make sure we got a location
            if (location.IsNullOrWhiteSpace())
                continue;

            // Add the game
            AddGame(game, location);
        }
    }

    /// <summary>
    /// Gets the data from the ubi.ini file
    /// </summary>
    /// <returns>A dictionary of the sections and their the install location value</returns>
    private IDictionary<string, string?> GetUbiIniData()
    {
        // Create the ini data parser
        return new FileIniDataParser(new UbiIniDataParser()).
            // Read the primary ubi.ini file
            ReadFile(AppFilePaths.UbiIniPath1).
            // Get the sections
            Sections.
            // Create a dictionary
            ToDictionary(x => x.SectionName, x => x.Keys.GetKeyData("Directory")?.Value);
    }

    /// <summary>
    /// Enumerates the program shortcuts from common locations such as the Start Menu and Desktop
    /// </summary>
    /// <returns>The program shortcut paths</returns>
    private IEnumerable<string> EnumerateProgramShortcuts()
    {
        // Get items from user start menu
        foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.StartMenu.GetFolderPath(), SearchOption.AllDirectories))
        {
            // Yield return the item
            yield return file;
        }

        Logger.Trace("The user start menu programs were retrieved for the game finder");

        // Get items from common start menu
        foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.CommonStartMenu.GetFolderPath(), SearchOption.AllDirectories))
        {
            // Yield return the item
            yield return file;
        }

        Logger.Trace("The common start menu programs were retrieved for the game finder");

        // Get items from user desktop
        foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.DesktopDirectory.GetFolderPath()))
        {
            // Yield return the item
            yield return file;
        }

        Logger.Trace("The user desktop shortcuts were retrieved for the game finder");

        // Get items from common desktop
        foreach (string file in EnumerateShortcuts(Environment.SpecialFolder.CommonDesktopDirectory.GetFolderPath()))
        {
            // Yield return the item
            yield return file;
        }

        Logger.Trace("The common desktop shortcuts were retrieved for the game finder");
    }

    /// <summary>
    /// Enumerates the program shortcuts from the specified directory
    /// </summary>
    /// <param name="directory">The directory to get the shortcuts from</param>
    /// <param name="searchOption">The search option to use</param>
    /// <returns>The program shortcut paths</returns>
    private IEnumerable<string> EnumerateShortcuts(FileSystemPath directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        try
        {
            // Get items from specified directory
            return Directory.EnumerateFiles(directory, "*.lnk", searchOption);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Enumerating shortcuts for game finder in {0}", directory);
                
            // Return an empty array to enumerate
            return new string[0];
        }
    }

    /// <summary>
    /// Enumerates the programs from the Registry uninstall key
    /// </summary>
    /// <returns>The programs</returns>
    private IEnumerable<InstalledProgram> EnumerateRegistryUninstallPrograms()
    {
        Logger.Info("Getting installed programs from the Registry");

        // Get 64-bit location if on 64-bit system
        RegistryKey?[] keys = Environment.Is64BitOperatingSystem
            ? new[]
            {
                RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry32),
                RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry64)
            }
            : new[]
            {
                RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Registry32),
            };

        // Enumerate the uninstall keys
        foreach (RegistryKey? key in keys)
        {
            if (key == null)
                continue;

            // Dispose key when done
            using RegistryKey registryKey = key;

            // Enumerate the sub keys
            foreach (string subKeyName in registryKey.GetSubKeyNames())
            {
                // Make sure it's not a Windows update
                if (subKeyName.StartsWith("KB") && subKeyName.Length == 8)
                    continue;

                // Open the sub key
                using RegistryKey? subKey = registryKey.OpenSubKey(subKeyName);

                // Make sure the key is not null    
                if (subKey == null)
                    continue;

                // Make sure it is not a system component
                if (subKey.GetValue("SystemComponent") as int? == 1)
                    continue;

                if (subKey.GetValue("WindowsInstaller") as int? == 1)
                    continue;

                // Make sure it has an uninstall string
                if (subKey.GetValue("UninstallString") == null)
                    continue;

                if (subKey.GetValue("ParentKeyName") != null)
                    continue;

                // Make sure it has a display name
                if (subKey.GetValue("DisplayName") is not string dn)
                    continue;

                // Make sure it has an install location
                if (subKey.GetValue("InstallLocation") is not string dir)
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
    /// <returns>True if the game item was added, otherwise false</returns>
    private bool AddGame(GameFinderItemContainer game, FileSystemPath installDir)
    {
        Logger.Info("An install directory was found for {0}", game.GameDescriptor.GameId);

        // Make sure the game hasn't already been found
        if (FoundGames.Contains(game.GameDescriptor))
        {
            Logger.Warn("{0} could not be added. The game has already been found.", game.GameDescriptor.GameId);
            return false;
        }

        // Make sure the install directory exists
        if (!installDir.DirectoryExists)
        {
            Logger.Warn("{0} could not be added. The install directory does not exist.", game.GameDescriptor.GameId);
            return false;
        }

        // If available, run custom verification
        if (game.FinderItem.VerifyInstallDirectory != null)
        {
            var result = game.FinderItem.VerifyInstallDirectory?.Invoke(installDir);

            if (result == null)
            {
                Logger.Info("{0} could not be added. The optional verification returned null.", game.GameDescriptor.GameId);
                return false;
            }

            installDir = result.Value;
        }

        // Make sure that the game is valid
        if (!game.GameDescriptor.IsValid(installDir))
        {
            Logger.Info("{0} could not be added. The game default file was not found.", game.GameDescriptor.GameId);
            return false;
        }

        // Add the game to found games
        Results.Add(new GameFinder_GameResult(game.GameDescriptor, installDir));

        // Remove from games to find
        GamesToFind.Remove(game.GameDescriptor);

        Logger.Info("The game {0} was found", game.GameDescriptor.GameId);

        return true;
    }

    /// <summary>
    /// Attempts to add a found <see cref="GameFinder_GenericItem"/>
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="installDir">The found install directory</param>
    /// <returns>True if the item was added, otherwise false</returns>
    private bool AddItem(GameFinder_GenericItem item, FileSystemPath installDir)
    {
        Logger.Info("An install directory was found for a finder item");

        // Make sure the install directory exists
        if (!installDir.DirectoryExists)
        {
            Logger.Warn("The item could not be added. The install directory does not exist.");
            return false;
        }

        // If available, run custom verification
        if (item.VerifyInstallDirectory != null)
        {
            var result = item.VerifyInstallDirectory?.Invoke(installDir);

            if (result == null)
            {
                Logger.Info("The item could not be added. The optional verification returned null.");
                return false;
            }

            installDir = result.Value;
        }

        // Add the item
        FoundFinderItems.Add(item);
        Results.Add(new GameFinder_GenericResult(installDir, item.FoundAction, item.DisplayName));

        Logger.Info("A finder item was found");

        return true;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Attempts to find the specified games, returning the found games and their install locations. This method can only be called once per class instance.
    /// </summary>
    /// <returns>The found games and their install locations</returns>
    public IReadOnlyList<GameFinder_BaseResult> FindGames()
    {
        if (HasRun)
            throw new Exception("The FindGames method can only be called once per instance");

        HasRun = true;

        try
        {
            // Split finders into groups
            var ubiIniGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.GameDescriptor) && x.FinderItem.UbiIniSectionName != null).ToArray();

            // Search the ubi.ini file
            if (ubiIniGameFinders.Any() && GamesToFind.Any())
            {
                IDictionary<string, string?>? iniLocations = null;

                Logger.Info("The game finder has ubi.ini finder");

                try
                {
                    // Make sure the file exists
                    if (AppFilePaths.UbiIniPath1.FileExists)
                    {
                        // Get the sections and the directory for each one
                        iniLocations = GetUbiIniData();
                        Logger.Info("The ubi.ini file data was parsed for the game finder");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Reading ubi.ini file for game finder");
                }

                // If we retrieved ini data, search it
                if (iniLocations != null)
                    SearchIniData(iniLocations, ubiIniGameFinders);
                else
                    Logger.Info("The ubi.ini file data was null");
            }

            // Split finders into groups
            var regUninstallGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.GameDescriptor) && x.FinderItem.PossibleWin32Names?.Any() == true).ToList();
            var regUninstallFinders = FinderItems.Where(x => !FoundFinderItems.Contains(x) && x.PossibleWin32Names?.Any() == true).ToList();
            var steamGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.GameDescriptor) && x.FinderItem.SteamID != null).ToList();

            // Search Registry uninstall programs
            if ((regUninstallGameFinders.Any() || steamGameFinders.Any() || regUninstallFinders.Any()) && GamesToFind.Any())
            {
                Logger.Info("The Registry uninstall programs are being searched...");

                try
                {
                    // Get the enumerator for installed programs
                    var installedPrograms = EnumerateRegistryUninstallPrograms();

                    // Search installed programs
                    SearchRegistryUninstall(installedPrograms, regUninstallGameFinders, steamGameFinders, regUninstallFinders);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Searching Registry uninstall programs for game finder");
                }
            }

            // Split finders into groups
            var programShortcutGameFinders = GameFinderItems.Where(x => !FoundGames.Contains(x.GameDescriptor) && x.FinderItem.ShortcutName != null).ToList();
            var programShortcutFinders = FinderItems.Where(x => !FoundFinderItems.Contains(x) && x.ShortcutName != null).ToList();

            // Search Win32 shortcuts
            if ((programShortcutGameFinders.Any() || programShortcutFinders.Any()) && GamesToFind.Any())
            {
                Logger.Info("The program shortcuts are being searched...");

                try
                {
                    // Get the enumerator for the shortcuts
                    var shortcuts = EnumerateProgramShortcuts();

                    // Search the shortcuts
                    SearchWin32Shortcuts(shortcuts, programShortcutGameFinders, programShortcutFinders);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Searching program shortcuts for game finder");
                }
            }

            // Run custom game finders
            foreach (GameFinderItemContainer game in GameFinderItems)
            {
                if (FoundGames.Contains(game.GameDescriptor) || game.FinderItem.CustomFinderAction == null)
                    continue;

                // Run the custom action and get the result
                GameFinder_FoundResult? result = game.FinderItem.CustomFinderAction();

                // Make sure we got a result
                if (result == null)
                    continue;

                // Add the game
                AddGame(game, result.InstallDir);
            }

            // Run custom finders
            foreach (GameFinder_GenericItem item in FinderItems)
            {
                if (FoundFinderItems.Contains(item) || item.CustomFinderAction == null)
                    continue;

                // Run the custom action and get the result
                GameFinder_FoundResult? result = item.CustomFinderAction();

                // Make sure we got a result
                if (result == null)
                    continue;

                // Add the game
                AddItem(item, result.InstallDir);
            }

            Logger.Info("The game finder found {0} games", Results.Count);

            // Return the found games
            return Results.AsReadOnly();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Game finder");
            throw;
        }
    }

    #endregion

    #region Private Classes

    /// <summary>
    /// An installed program
    /// </summary>
    private class InstalledProgram
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
        public string? SteamID { get; }
    }

    /// <summary>
    /// An container for a <see cref="GameFinder_GameItem"/> with the game and type
    /// </summary>
    private class GameFinderItemContainer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="gameDescriptor">The game to find</param>
        /// <param name="finderItem">The game finder item</param>
        public GameFinderItemContainer(GameDescriptor gameDescriptor, GameFinder_GameItem finderItem)
        {
            GameDescriptor = gameDescriptor;
            FinderItem = finderItem;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game to find
        /// </summary>
        public GameDescriptor GameDescriptor { get; }

        /// <summary>
        /// The game finder item
        /// </summary>
        public GameFinder_GameItem FinderItem { get; }

        #endregion
    }

    #endregion
}