using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles common actions and events for this application
    /// </summary>
    public class AppHandler
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppHandler()
        {
            SaveUserDataAsyncLock = new AsyncLock();
        }

        #endregion

        #region Constant Fields

        /// <summary>
        /// The base path for this application
        /// </summary>
        public const string ApplicationBasePath = "pack://application:,,,/RayCarrot.RCP.Metro;component/";

        /// <summary>
        /// The Steam store base url
        /// </summary>
        public const string SteamStoreBaseUrl = "https://store.steampowered.com/app/";

        #endregion

        #region Public Properties

        /// <summary>
        /// An async lock for the <see cref="SaveUserDataAsync"/> method
        /// </summary>
        private AsyncLock SaveUserDataAsyncLock { get; }

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentVersion => new Version(0, 0, 0, 0);

        /// <summary>
        /// Gets a collection of the available <see cref="Games"/>
        /// </summary>
        public IEnumerable<Games> GetGames => Enum.GetValues(typeof(Games)).Cast<Games>();

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a list of installed programs from the Registry uninstall paths
        /// </summary>
        /// <returns>The installed programs</returns>
        private static IEnumerable<InstalledProgram> GetInstalledPrograms()
        {
            RCF.Logger.LogInformationSource("Getting installed programs from the Registry");

            var output = new List<InstalledProgram>();

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
                using (key)
                {
                    // Enumerate the sub keys
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        // Make sure it's not a Windows update
                        if (subKeyName.StartsWith("KB") && subKeyName.Length == 8)
                            continue;

                        // Open the sub key
                        using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                        {
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

                            output.Add(new InstalledProgram(dn, subKey.Name, dir));
                        }
                    }
                }
            }

            RCF.Logger.LogInformationSource($"Found {output.Count} installed programs");

            return output;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Fires the <see cref="RefreshRequired"/> event
        /// </summary>
        /// <param name="e">The event arguments, or null to use the default ones</param>
        protected void OnRefreshRequired(EventArgs e = null)
        {
            RefreshRequired?.Invoke(this, e ?? EventArgs.Empty);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new game to the app data
        /// </summary>
        /// <param name="game">The game to add</param>
        /// <param name="type">The game type</param>
        /// <param name="installDirectory">The game install directory, if available</param>
        /// <returns>The task</returns>
        public async Task AddNewGameAsync(Games game, GameType type, FileSystemPath? installDirectory = null)
        {
            RCF.Logger.LogInformationSource($"The game {game} is being added of type {type}...");

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                RCF.Logger.LogWarningSource($"The game {game} has already been added");

                await RCF.MessageUI.DisplayMessageAsync($"The game {game} has already been added", "Error adding new game", MessageType.Error);

                return;
            }

            // Get the install directory
            if (installDirectory == null)   
            {
                if (type == GameType.Steam)
                {
                    try
                    {
                        // Get the key path
                        var keyPath = RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {game.GetSteamID()}");

                        using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(keyPath, RegistryView.Registry64))
                            installDirectory = key?.GetValue("InstallLocation") as string;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Getting Steam game install directory");
                    }
                }
                else if (type == GameType.WinStore)
                {
                    // TODO: Use Win10 API in .NET 4.8
                }
                else
                {
                    // TODO: Handle error
                    return;
                }

                RCF.Logger.LogInformationSource($"The game {game} install directory was retrieved as {installDirectory}");
            }

            // Add the game
            RCFRCP.Data.Games.Add(game, new GameInfo(type, installDirectory ?? FileSystemPath.EmptyPath));

            // If it's a DosBox game, add the DosBox options
            if (type == GameType.DosBox && !RCFRCP.Data.DosBoxGames.ContainsKey(game))
                RCFRCP.Data.DosBoxGames.Add(game, new DosBoxOptions());

            RCF.Logger.LogInformationSource($"The game {game} has been added");

            // Refresh
            OnRefreshRequired();
        }

        /// <summary>
        /// Removes the specified game
        /// </summary>
        /// <param name="game">The game to remove</param>
        public void RemoveGame(Games game)
        {
            // Remove the game
            RCFRCP.Data.Games.Remove(game);

            // If there is DosBox options saved, remove those as well
            if (RCFRCP.Data.DosBoxGames.ContainsKey(game))
                RCFRCP.Data.DosBoxGames.Remove(game);

            // Refresh the games
            RCFRCP.App.OnRefreshRequired();
        }

        /// <summary>
        /// Saves all user data for the application
        /// </summary>
        public async Task SaveUserDataAsync()
        {
            // Lock the saving of user data
            using (await SaveUserDataAsyncLock.LockAsync())
            {
                try
                {
                    // Run it as a new task
                    await Task.Run(async () =>
                    {
                        // Save all user data
                        await RCFData.UserDataCollection.SaveAllAsync();
                    });

                    RCF.Logger.LogInformationSource($"The application user data was saved");
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Saving user data");
                }
            }
        }

        /// <summary>
        /// Resets all user data for the application
        /// </summary>
        /// <returns></returns>
        public void ResetData()
        {
            RCFData.UserDataCollection.ForEach(x => x.Reset());

            RCF.Logger.LogInformationSource($"The application user data was reset");

            OnRefreshRequired();
        }

        /// <summary>
        /// Enables write access to the primary ubi.ini file
        /// </summary>
        /// <returns>The task</returns>
        public async Task EnableUbiIniWriteAccessAsync()
        {
            try
            {
                if (!CommonPaths.UbiIniPath1.FileExists)
                {
                    // TODO: Handle/Log
                    return;
                }

                // Check if we have write access
                if (RCFRCP.File.CheckFileWriteAccess(CommonPaths.UbiIniPath1))
                    return;

                // TODO: Log

                await RCF.MessageUI.DisplayMessageAsync("To be able to configure the Rayman games without running this program as administrator you will " +
                                                        "need to accept the following admin prompt");

                // Attempt to change the permission through CMD to avoid having to run RCP as admin
                // ReSharper disable once StringLiteralTypo
                WindowsHelpers.RunCommandPromptScript($"icacls \"{CommonPaths.UbiIniPath1}\" /grant Users:W", true);

                RCF.Logger.LogInformationSource($"The ubi.ini file permission was changed");
            }
            catch (Exception ex)
            {
                ex.HandleError("Changing ubi.ini file permissions");
                // TODO: Handle/Log
            }
        }

        /// <summary>
        /// Checks for installed games
        /// </summary>
        /// <returns>True if new games were found, otherwise false</returns>
        public async Task<bool> RunGameFinderAsync()
        {
            // TODO: Have way to find WinStore games

            try
            {
                // Create the manager
                var manager = new GameFinderManager();

                // Helper method for checking if the specified games are being checked for
                bool AreGamesIncluded(params Games[] games)
                {
                    return games.Any(game => manager.GameItems.ContainsKey(game));
                }

                // The ubi.ini data
                IniData iniData = null;
                try
                {
                    // Make sure the following games are being checked for
                    if (AreGamesIncluded(Games.Rayman2, Games.RaymanM, Games.RaymanArena, Games.Rayman3))
                    {
                        // Make sure the file exists
                        if (CommonPaths.UbiIniPath1.FileExists)
                        {
                            // Create the ini data parser
                            iniData = new FileIniDataParser(new UbiIniDataParser()).ReadFile(CommonPaths.UbiIniPath1);

                            RCF.Logger.LogTraceSource("The ubi.ini file data was parsed for the game checker");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Reading ubi.ini file for game checker");
                }

                // Registry uninstall programs
                List<InstalledProgram> programs = new List<InstalledProgram>();
                try
                {
                    if (manager.GameItems.Any())
                    {
                        // Get installed programs
                        programs.AddRange(GetInstalledPrograms());

                        RCF.Logger.LogTraceSource("The Registry uninstall programs were retrieved for the game checker");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting installed programs for game checker");
                }

                // Start menu items
                List<string> startMenuItems = new List<string>();
                try
                {
                    if (manager.GameItems.Any())
                    {
                        // Get items from user start menu
                        startMenuItems.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "*.lnk", SearchOption.AllDirectories));

                        RCF.Logger.LogTraceSource("The user start menu programs were retrieved for the game checker");

                        // Get items from common start menu
                        startMenuItems.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "*.lnk", SearchOption.AllDirectories));

                        RCF.Logger.LogTraceSource("The common start menu programs were retrieved for the game checker");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting start menu programs for game checker");
                }

                // Helper methods
                GameFinderActionResult CheckUbiIni(string sectionName, GameType type = GameType.Win32) => new GameFinderActionResult(iniData?[sectionName]?["Directory"], type, "ubi.ini");
                GameFinderActionResult CheckUninstall(params string[] names)
                {
                    var program = programs.Find(x => x.DisplayName.Equals(StringComparison.CurrentCultureIgnoreCase, names));
                    var path = program?.InstallLocation ?? FileSystemPath.EmptyPath;
                    var type = program == null ? GameType.Win32 : program.IsSteamGame ? GameType.Steam : GameType.Win32;
                    return new GameFinderActionResult(path, type, "RegistryUninstall");
                }
                GameFinderActionResult CheckStartMenu(string name, string requiredFile)
                {
                    foreach (string startMenuItem in startMenuItems)
                    {
                        if (Path.GetFileName(startMenuItem)?.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) < 0)
                            continue;

                        FileSystemPath path;

                        try
                        {
                            path = WindowsHelpers.GetShortCutTarget(startMenuItem).Parent;
                        }
                        catch (Exception ex)
                        {
                            ex.HandleUnexpected("Getting start menu item shortcut target for game checker", startMenuItem);
                            continue;
                        }

                        if (!path.DirectoryExists || !(path + requiredFile).FileExists)
                            continue;

                        return new GameFinderActionResult(path, GameType.Win32, "StartMenu");
                    }

                    return new GameFinderActionResult(FileSystemPath.EmptyPath, GameType.Win32, "None");
                }

                // Helper method for adding actions
                void AddActions(Games game, IEnumerable<Func<GameFinderActionResult>> actions)
                {
                    // Make sure the game is being checked for
                    if (!manager.GameItems.ContainsKey(game))
                        return;

                    manager.GameItems[game].AddRange(actions);
                }

                // Rayman 2
                AddActions(Games.Rayman2, new Func<GameFinderActionResult>[]
                {
                    // Ubi.ini
                    () => CheckUbiIni(R2UbiIniHandler.SectionName),

                    // Uninstall
                    () => CheckUninstall("Rayman 2 - The Great Escape", "Rayman 2", "Rayman2", "GOG.com Rayman 2"),

                    // Start menu
                    () => CheckStartMenu("Rayman 2", Games.Rayman2.GetLaunchName()),
                });

                // Rayman M
                AddActions(Games.RaymanM, new Func<GameFinderActionResult>[]
                {
                    // Ubi.ini
                    () => CheckUbiIni(RMUbiIniHandler.SectionName),
                            
                    // Uninstall
                    () => CheckUninstall("Rayman M", "RaymanM"),

                    // Start menu
                    () => CheckStartMenu("Rayman M", Games.RaymanM.GetLaunchName()),
                });

                // Rayman Arena
                AddActions(Games.RaymanArena, new Func<GameFinderActionResult>[]
                {
                    // Ubi.ini
                    () => CheckUbiIni(RAUbiIniHandler.SectionName),
                        
                    // Uninstall
                    () => CheckUninstall("Rayman Arena", "RaymanArena"),

                    // Start menu
                    () => CheckStartMenu("Rayman Arena", Games.RaymanArena.GetLaunchName()),
                });

                // Rayman 3
                AddActions(Games.Rayman3, new Func<GameFinderActionResult>[]
                {
                    // Ubi.ini
                    () => CheckUbiIni(R3UbiIniHandler.SectionName),

                    // Uninstall
                    () => CheckUninstall("Rayman 3 - Hoodlum Havoc", "Rayman 3", "Rayman3"),

                    // Start menu
                    () => CheckStartMenu("Rayman 3", Games.Rayman3.GetLaunchName()),
                });

                // Rayman Raving Rabbids
                AddActions(Games.RaymanRavingRabbids, new Func<GameFinderActionResult>[]
                {
                    // Uninstall
                    () => CheckUninstall("Rayman: Raving Rabbids", "Rayman Raving Rabbids"),

                    // Start menu
                    () => CheckStartMenu("Rayman Raving Rabbids", Games.RaymanRavingRabbids.GetLaunchName()),
                });

                // Rayman Origins
                AddActions(Games.RaymanOrigins, new Func<GameFinderActionResult>[]
                {
                    // Uninstall
                    () => CheckUninstall("Rayman Origins", "RaymanOrigins"),

                    // Start menu
                    () => CheckStartMenu("Rayman Origins", Games.RaymanOrigins.GetLaunchName()),
                });

                // Rayman Legends
                AddActions(Games.RaymanLegends, new Func<GameFinderActionResult>[]
                {
                    // Uninstall
                    () => CheckUninstall("Rayman Legends", "RaymanLegends"),

                    // Start menu
                    () => CheckStartMenu("Rayman Legends", Games.RaymanLegends.GetLaunchName()),
                });

                // Run the checker and get the results
                var result = await manager.RunAsync();

                if (!File.Exists(RCFRCP.Data.DosBoxPath))
                    FindDosBox();

                void FindDosBox()
                {
                    var actions = new Func<GameFinderActionResult>[]
                    {
                        // Uninstall
                        () => CheckUninstall("DosBox", "Dos Box"),

                        // Start menu
                        () => CheckStartMenu("DosBox", "DOSBox.exe"),
                    };

                    // Run every check action until one is successful
                    foreach (var action in actions)
                    {
                        // Stop running the check actions if the file has been found
                        if (File.Exists(RCFRCP.Data.DosBoxPath))
                            break;

                        try
                        {
                            // Get the result from the action
                            var dosBoxCheckResult = action();

                            var filePath = dosBoxCheckResult.Path + "DOSBox.exe";

                            // Check if the file exists
                            if (!filePath.FileExists)
                                continue;

                            RCF.Logger.LogTraceSource($"The DosBox executable was found from the game checker with the source {dosBoxCheckResult.Source}");

                            RCFRCP.Data.DosBoxPath = filePath;

                            break;
                        }
                        catch (Exception ex)
                        {
                            ex.HandleUnexpected("DosBox check action", action);
                        }
                    }
                }

                // 

                // Check Rayman Forever
                if (!Games.Rayman1.IsAdded() &&
                    !Games.Rayman1.IsAdded() &&
                    !Games.RaymanDesigner.IsAdded() &&
                    !Games.RaymanByHisFans.IsAdded())
                {
                    var dir = CheckUninstall("Rayman Forever").Path;

                    FileSystemPath mountFileA = Path.Combine(dir, "game.inst");
                    FileSystemPath mountFileB = Path.Combine(dir, "Music\\game.inst");

                    if ((mountFileA.FileExists || mountFileB.FileExists) &&
                        File.Exists(Path.Combine(dir, "Rayman\\RAYMAN.EXE")) &&
                        File.Exists(Path.Combine(dir, "RayKit\\RayKit.exe")) &&
                        File.Exists(Path.Combine(dir, "RayFan\\RayFan.exe")) &&
                        File.Exists(Path.Combine(dir, "DosBox\\DOSBox.exe")) &&
                        File.Exists(Path.Combine(dir, "dosboxRayman.conf")))
                    {
                        result.InsertRange(0, new Games[]
                        {
                            Games.Rayman1,
                            Games.RaymanDesigner,
                            Games.RaymanByHisFans
                        });

                        if (!File.Exists(RCFRCP.Data.DosBoxPath))
                            RCFRCP.Data.DosBoxPath = Path.Combine(dir, "DosBox\\DOSBox.exe");

                        RCFRCP.Data.DosBoxConfig = Path.Combine(dir, "dosboxRayman.conf");

                        // Add the games
                        await RCFRCP.App.AddNewGameAsync(Games.Rayman1, GameType.DosBox, dir + "Rayman");
                        await RCFRCP.App.AddNewGameAsync(Games.RaymanDesigner, GameType.DosBox, dir + "RayKit");
                        await RCFRCP.App.AddNewGameAsync(Games.RaymanByHisFans, GameType.DosBox, dir + "RayFan");

                        RCF.Logger.LogInformationSource($"The games in Rayman Forever has been added from the game finder");

                        var mountPath = mountFileA.FileExists ? mountFileA : mountFileB;

                        RCFRCP.Data.DosBoxGames[Games.Rayman1].MountPath = mountPath;
                        RCFRCP.Data.DosBoxGames[Games.RaymanDesigner].MountPath = mountPath;
                        RCFRCP.Data.DosBoxGames[Games.RaymanByHisFans].MountPath = mountPath;
                    }
                }

                if (result.Count > 0)
                {
                    await RCF.MessageUI.DisplayMessageAsync($"The following new games were found:{Environment.NewLine}{Environment.NewLine}• {result.JoinItems(Environment.NewLine + "• ", x => x.GetDisplayName())}", "Installed games found", MessageType.Success);
                    RCF.Logger.LogInformationSource($"The game finder found the following games {result.JoinItems(", ")}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Game finder");
                await RCF.MessageUI.DisplayMessageAsync("An error occurred during the game finder operation");
            }
            finally
            {
                await RCFRCP.App.SaveUserDataAsync();
            }

            return false;
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

                // Check if it's a Steam app
                IsSteamGame = registryKeyPath.Contains("Steam App");
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
            public bool IsSteamGame { get; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a refresh is required for the games
        /// </summary>
        public event EventHandler RefreshRequired;

        #endregion
    }
}