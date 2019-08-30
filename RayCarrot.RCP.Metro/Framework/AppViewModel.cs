using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ByteSizeLib;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles common actions and events for this application
    /// </summary>
    public class AppViewModel : BaseRCPViewModel
    {
        #region Static Constructor

        static AppViewModel()
        {
            WindowsVersion = WindowsHelpers.GetCurrentWindowsVersion();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppViewModel()
        {
            try
            {
                IsRunningAsAdmin = WindowsHelpers.RunningAsAdmin;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                IsRunningAsAdmin = false;
            }

            SaveUserDataAsyncLock = new AsyncLock();
            MoveBackupsAsyncLock = new AsyncLock();
            AdminWorkerAsyncLock = new AsyncLock();
            OnRefreshRequiredAsyncLock = new AsyncLock();

            RestartAsAdminCommand = new AsyncRelayCommand(RestartAsAdminAsync);
            RequestRestartAsAdminCommand = new AsyncRelayCommand(RequestRestartAsAdminAsync);

            LocalUtilities = new Dictionary<Games, Type[]>()
            {
                {
                    Games.Rayman1,
                    new Type[]
                    {
                        typeof(R1TPLSUtility),
                        typeof(R1CompleteSoundtrackUtility),
                    }
                },
                {
                    Games.RaymanDesigner,
                    new Type[]
                    {
                        typeof(RDReplaceFilesUtility),
                        typeof(RDCreateConfigUtility),
                    }
                },
                {
                    Games.Rayman2,
                    new Type[]
                    {
                        typeof(R2TranslationUtility),
                    }
                },
                {
                    Games.Rayman3,
                    new Type[]
                    {
                        typeof(R3DirectPlayUtility),
                    }
                },
                {
                    Games.RaymanOrigins,
                    new Type[]
                    {
                        typeof(ROHQVideosUtility),
                        typeof(RODebugCommandsUtility),
                        typeof(ROUpdateUtility),
                    }
                },
                {
                    Games.RaymanLegends,
                    new Type[]
                    {
                        typeof(RLUbiRayUtility),
                    }
                },
            };
        }

        #endregion

        #region Commands

        public ICommand RestartAsAdminCommand { get; }

        public ICommand RequestRestartAsAdminCommand { get; }

        #endregion

        #region Constant Fields

        /// <summary>
        /// The base path for this application
        /// </summary>
        public const string ApplicationBasePath = "pack://application:,,,/RayCarrot.RCP.Metro;component/";

        /// <summary>
        /// The Steam store base URL
        /// </summary>
        public const string SteamStoreBaseUrl = "https://store.steampowered.com/app/";

        /// <summary>
        /// The name of the backup directory for this application
        /// </summary>
        public const string BackupFamily = "Rayman Game Backups";

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The Windows version the program is running on
        /// </summary>
        public static WindowsVersion WindowsVersion { get; }

        #endregion

        #region Private Properties

        /// <summary>
        /// An async lock for the <see cref="SaveUserDataAsync"/> method
        /// </summary>
        private AsyncLock SaveUserDataAsyncLock { get; }

        /// <summary>
        /// An async lock for the <see cref="MoveBackupsAsync"/> method
        /// </summary>
        private AsyncLock MoveBackupsAsyncLock { get; }

        /// <summary>
        /// An async lock for the <see cref="RunAdminWorkerAsync"/> method
        /// </summary>
        private AsyncLock AdminWorkerAsyncLock { get; }

        /// <summary>
        /// An async lock for the <see cref="OnRefreshRequiredAsync"/> method
        /// </summary>
        private AsyncLock OnRefreshRequiredAsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The available local utilities
        /// </summary>
        public Dictionary<Games, Type[]> LocalUtilities { get; }

        /// <summary>
        /// The currently selected page
        /// </summary>
        public Pages SelectedPage { get; set; }

        /// <summary>
        /// A flag indicating if an update check is in progress
        /// </summary>
        public bool CheckingForUpdates { get; set; }

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentVersion => new Version(6, 0, 0, 0);

        /// <summary>
        /// Indicates if the current version is a beta version
        /// </summary>
        public bool IsBeta => true;

        /// <summary>
        /// Gets a collection of the available <see cref="Games"/>
        /// </summary>
        public IEnumerable<Games> GetGames => Enum.GetValues(typeof(Games)).Cast<Games>();

        /// <summary>
        /// Indicates if the game finder is currently running
        /// </summary>
        public bool IsGameFinderRunning { get; set; }

        /// <summary>
        /// Indicates if the program is running as admin
        /// </summary>
        public bool IsRunningAsAdmin { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a list of installed programs from the Registry uninstall paths
        /// </summary>
        /// <returns>The installed programs</returns>
        private static IEnumerable<InstalledProgram> GetInstalledPrograms()
        {
            RCFCore.Logger?.LogInformationSource("Getting installed programs from the Registry");

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

            RCFCore.Logger?.LogInformationSource($"Found {output.Count} installed programs");

            return output;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets new instances of utilities for a specific game
        /// </summary>
        /// <param name="game">The game to get the utilities for</param>
        /// <returns>The utilities instances</returns>
        public IEnumerable<IRCPUtility> GetUtilities(Games game)
        {
            var utilities = LocalUtilities.TryGetValue(game);

            if (utilities == null)
                return new IRCPUtility[0]; 

            return utilities.
                // Create a new instance of each utility
                Select(x => x.CreateInstance<IRCPUtility>()).
                // Make sure it's available
                Where(x => x.IsAvailable);
        }

        /// <summary>
        /// Fires the <see cref="RefreshRequired"/> event
        /// </summary>
        /// <returns>The task</returns>
        public async Task OnRefreshRequiredAsync(RefreshRequiredEventArgs eventArgs)
        {
            using (await OnRefreshRequiredAsyncLock.LockAsync())
            {
                RCFCore.Logger?.LogDebugSource("A refresh is being requested");

                await (RefreshRequired?.RaiseAsync(this, eventArgs) ?? Task.CompletedTask);
            }
        }

        /// <summary>
        /// Adds a new game to the app data
        /// </summary>
        /// <param name="game">The game to add</param>
        /// <param name="type">The game type</param>
        /// <param name="installDirectory">The game install directory, if available</param>
        /// <returns>The task</returns>
        public async Task AddNewGameAsync(Games game, GameType type, FileSystemPath? installDirectory = null)
        {
            RCFCore.Logger?.LogInformationSource($"The game {game} is being added of type {type}...");

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                RCFCore.Logger?.LogWarningSource($"The game {game} has already been added");

                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, game), Resources.AddGame_DuplicateHeader, MessageType.Error);

                return;
            }

            // Get the install directory
            if (installDirectory == null)   
            {
                // Attempt to get path from game manager
                installDirectory = game.GetGameManager(type).GetInstallDirectory();

                RCFCore.Logger?.LogInformationSource($"The game {game} install directory was retrieved as {installDirectory}");
            }

            // Add the game
            Data.Games.Add(game, new GameInfo(type, installDirectory.Value));

            RCFCore.Logger?.LogInformationSource($"The game {game} has been added");

            // Run post-add operations
            await game.GetGameManager().PostGameAddAsync();
        }

        /// <summary>
        /// Removes the specified game
        /// </summary>
        /// <param name="game">The game to remove</param>
        /// <param name="forceRemove">Indicates if the game should be force removed</param>
        /// <returns>The task</returns>
        public async Task RemoveGameAsync(Games game, bool forceRemove)
        {
            try
            {
                // Get the game type
                var type = game.GetInfo().GameType;

                if (!forceRemove)
                {
                    // Get applied utilities
                    var utilities = await game.GetAppliedUtilitiesAsync();

                    // Warn about utilities
                    if (utilities.Any() && !await RCFUI.MessageUI.DisplayMessageAsync(
                            $"{Resources.RemoveGame_UtilityWarning}{Environment.NewLine}{Environment.NewLine}{utilities.JoinItems(Environment.NewLine)}", Resources.RemoveGame_UtilityWarningHeader, MessageType.Warning, true))
                        return;
                }

                // Remove the game
                Data.Games.Remove(game);

                // Run post game removal
                await game.GetGameManager(type).PostGameRemovedAsync();
            }
            catch (Exception ex)
            {
                ex.HandleCritical("Removing game");
                throw;
            }
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

                    RCFCore.Logger?.LogInformationSource($"The application user data was saved");
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Saving user data");
                }
            }
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
                    RCFCore.Logger?.LogInformationSource("The ubi.ini file was not found");
                    return;
                }

                // Check if we have write access
                if (RCFRCP.File.CheckFileWriteAccess(CommonPaths.UbiIniPath1))
                {
                    RCFCore.Logger?.LogDebugSource("The ubi.ini file has write access");
                    return;
                }

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

                // Attempt to change the permission
                await RunAdminWorkerAsync(AdminWorkerModes.GrantFullControl, CommonPaths.UbiIniPath1);

                RCFCore.Logger?.LogInformationSource($"The ubi.ini file permission was changed");
            }
            catch (Exception ex)
            {
                ex.HandleError("Changing ubi.ini file permissions");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_Error, MessageType.Error);
            }
        }

        /// <summary>
        /// Checks for installed games
        /// </summary>
        /// <returns>True if new games were found, otherwise false</returns>
        public async Task<bool> RunGameFinderAsync()
        {
            if (IsGameFinderRunning)
                return false;

            IsGameFinderRunning = true;

            return await Task.Run<bool>(async () =>
            {
                // Keep track of added games
                List<Games> addedGames = new List<Games>();

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

                                RCFCore.Logger?.LogTraceSource("The ubi.ini file data was parsed for the game checker");
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

                            RCFCore.Logger?.LogTraceSource("The Registry uninstall programs were retrieved for the game checker");
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

                            RCFCore.Logger?.LogTraceSource("The user start menu programs were retrieved for the game checker");

                            // Get items from common start menu
                            startMenuItems.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "*.lnk", SearchOption.AllDirectories));

                            RCFCore.Logger?.LogTraceSource("The common start menu programs were retrieved for the game checker");
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

                    // Rayman Raving Rabbids 2
                    AddActions(Games.RaymanRavingRabbids2, new Func<GameFinderActionResult>[]
                    {
                        // Uninstall
                        () => CheckUninstall("Rayman: Raving Rabbids 2", "Rayman Raving Rabbids 2"),

                        // Start menu
                        () => CheckStartMenu("Rayman Raving Rabbids 2", Games.RaymanRavingRabbids2.GetLaunchName()),
                    });

                    // Rabbids Go Home
                    AddActions(Games.RabbidsGoHome, new Func<GameFinderActionResult>[]
                    {
                        // Uninstall
                        () => CheckUninstall("Rabbids Go Home"),

                        // Start menu
                        () => CheckStartMenu("Rabbids Go Home", Games.RabbidsGoHome.GetLaunchName()),
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
                    addedGames.AddRange(await manager.RunAsync());

                    if (!File.Exists(Data.DosBoxPath))
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
                            if (File.Exists(Data.DosBoxPath))
                                break;

                            try
                            {
                                // Get the result from the action
                                var dosBoxCheckResult = action();

                                var filePath = dosBoxCheckResult.Path + "DOSBox.exe";

                                // Check if the file exists
                                if (!filePath.FileExists)
                                    continue;

                                RCFCore.Logger?.LogTraceSource($"The DosBox executable was found from the game checker with the source {dosBoxCheckResult.Source}");

                                Data.DosBoxPath = filePath;

                                break;
                            }
                            catch (Exception ex)
                            {
                                ex.HandleUnexpected("DosBox check action", action);
                            }
                        }
                    }

                    // Helper method for finding and adding a Windows Store app
                    async Task FindWinStoreAppAsync(Games game)
                    {
                        // Check if the game is installed
                        if (await game.GetGameManager(GameType.WinStore).IsValidAsync(FileSystemPath.EmptyPath))
                        {
                            addedGames.Add(game);

                            // Add the game
                            await AddNewGameAsync(game, GameType.WinStore);

                            RCFCore.Logger?.LogInformationSource($"The game {game.GetDisplayName()} has been added from the game finder");
                        }
                    }

                    // Check Windows Store apps
                    if (!Games.RaymanJungleRun.IsAdded())
                        await FindWinStoreAppAsync(Games.RaymanJungleRun);

                    if (!Games.RabbidsBigBang.IsAdded())
                        await FindWinStoreAppAsync(Games.RabbidsBigBang);

                    // Get the fiesta run manager
                    var fiestaRunManager = Games.RaymanFiestaRun.GetGameManager<WinStoreGameManager>();

                    foreach (FiestaRunEdition version in Enum.GetValues(typeof(FiestaRunEdition)))
                    {
                        if (Games.RaymanFiestaRun.IsAdded())
                            break;

                        // Add the game if it's found
                        if (fiestaRunManager.GetGamePackage(fiestaRunManager.GetFiestaRunPackageName(version)) != null)
                        {
                            addedGames.Add(Games.RaymanFiestaRun);

                            // Add the game
                            await AddNewGameAsync(Games.RaymanFiestaRun, GameType.WinStore);

                            // Set the version
                            RCFRCP.Data.FiestaRunVersion = version;

                            RCFCore.Logger?.LogInformationSource($"The game {Games.RaymanFiestaRun.GetDisplayName()} has been added from the game finder");
                        }
                    }

                    // Check Rayman Forever
                    if (!Games.Rayman1.IsAdded() &&
                        !Games.RaymanDesigner.IsAdded() &&
                        !Games.RaymanByHisFans.IsAdded())
                    {
                        var dir = CheckUninstall("Rayman Forever").Path;

                        FileSystemPath[] mountFiles = 
                        {
                            dir + "game.inst",
                            dir + "Music\\game.inst",
                            dir + "game.ins",
                            dir + "Music\\game.ins",
                        };

                        if (mountFiles.Any(x => x.FileExists) &&
                            File.Exists(dir + "Rayman" + Games.Rayman1.GetLaunchName()) &&
                            File.Exists(dir + "RayKit" + Games.RaymanDesigner.GetLaunchName()) &&
                            File.Exists(dir + "RayFan" + Games.RaymanByHisFans.GetLaunchName()) &&
                            File.Exists(dir + "DosBox" + "DOSBox.exe") &&
                            File.Exists(dir + "dosboxRayman.conf"))
                        {
                            addedGames.InsertRange(0, new Games[]
                            {
                                Games.Rayman1,
                                Games.RaymanDesigner,
                                Games.RaymanByHisFans
                            });

                            if (!File.Exists(Data.DosBoxPath))
                                Data.DosBoxPath = dir + "DosBox" + "DOSBox.exe";

                            Data.DosBoxConfig = dir + "dosboxRayman.conf";

                            // Add the games
                            await AddNewGameAsync(Games.Rayman1, GameType.DosBox, dir + "Rayman");
                            await AddNewGameAsync(Games.RaymanDesigner, GameType.DosBox, dir + "RayKit");
                            await AddNewGameAsync(Games.RaymanByHisFans, GameType.DosBox, dir + "RayFan");

                            RCFCore.Logger?.LogInformationSource($"The games in Rayman Forever has been added from the game finder");

                            var mountPath = mountFiles.FindItem(x => x.FileExists);

                            Data.DosBoxGames[Games.Rayman1].MountPath = mountPath;
                            Data.DosBoxGames[Games.RaymanDesigner].MountPath = mountPath;
                            Data.DosBoxGames[Games.RaymanByHisFans].MountPath = mountPath;
                        }
                    }

                    if (addedGames.Count > 0)
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {addedGames.JoinItems(Environment.NewLine + "• ", x => x.GetDisplayName())}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);
                        RCFCore.Logger?.LogInformationSource($"The game finder found the following games {addedGames.JoinItems(", ")}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Game finder");
                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.GameFinder_Error, MessageType.Error);
                }
                finally
                {
                    // Refresh if any games were added
                    if (addedGames.Any())
                        await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(addedGames, true, false, false, false));

                    await SaveUserDataAsync();
                    IsGameFinderRunning = false;
                }

                return false;
            });
        }

        /// <summary>
        /// Downloads the specified files to a specified output directory
        /// </summary>
        /// <param name="inputSources">The files to download</param>
        /// <param name="isCompressed">True if the download is a compressed file, otherwise false</param>
        /// <param name="outputDir">The output directory to download to</param>
        /// <returns>True if the download succeeded, otherwise false</returns>
        public async Task<bool> DownloadAsync(IList<Uri> inputSources, bool isCompressed, FileSystemPath outputDir)
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"A download is starting...");

                // Make sure the directory exists
                if (!outputDir.DirectoryExists)
                    Directory.CreateDirectory(outputDir);

                // Make sure there are input sources to download
                if (!inputSources.Any())
                {
                    RCFCore.Logger?.LogInformationSource($"Download failed due to there not being any input sources");

                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.Download_NoFilesFound, MessageType.Error);
                    return false;
                }

                // Allow user to confirm
                try
                {
                    ByteSize size = new ByteSize(0);
                    foreach (var item in inputSources)
                    {
                        var webRequest = WebRequest.Create(item);
                        webRequest.Method = "HEAD";

                        using (var webResponse = webRequest.GetResponse())
                            size = size.Add(new ByteSize(Convert.ToDouble(webResponse.Headers.Get("Content-Length"))));
                    }

                    RCFCore.Logger?.LogDebugSource($"The size of the download has been retrieved as {size}");

                    if (!await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Download_ConfirmSize, size), Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting download size");
                    if (!await RCFUI.MessageUI.DisplayMessageAsync(Resources.Download_Confirm, Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }

                // Create the download dialog
                var dialog = new Downloader(isCompressed ? new DownloaderViewModel(inputSources.First(), outputDir) : new DownloaderViewModel(inputSources, outputDir));

                // Show the dialog
                dialog.ShowDialog();

                RCFCore.Logger?.LogInformationSource($"The download finished with the result of {dialog.ViewModel.DownloadState}");

                // Return the result
                return dialog.ViewModel.DownloadState == DownloadState.Succeeded;
            }
            catch (Exception ex)
            {
                ex.HandleError($"Downloading files");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.Download_Error, MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Checks for application updates
        /// </summary>
        /// <param name="isManualSearch">Indicates if this is a manual check, in which cause a message should be shown if no update is found</param>
        /// <returns>The task</returns>
        public async Task CheckForUpdatesAsync(bool isManualSearch)
        {
            if (CheckingForUpdates)
                return;

            try
            {
                RCFCore.Logger?.LogInformationSource($"Updates are being checked for");

                CheckingForUpdates = true;
                string errorMessage = Resources.Update_UnknownError;
                JObject manifest = null;

                try
                {
                    using (var wc = new WebClient())
                    {
                        var result = await wc.DownloadStringTaskAsync(CommonUrls.UpdateManifestUrl);
                        manifest = JObject.Parse(result);
                    }
                }
                catch (WebException ex)
                {
                    ex.HandleUnexpected("Getting server manifest");
                    errorMessage = Resources.Update_WebError;
                }
                catch (JsonReaderException ex)
                {
                    ex.HandleError("Parsing server manifest");
                    errorMessage = Resources.Update_FormatError;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting server manifest");
                    errorMessage = Resources.Update_GenericError;
                }

                // Show error if manifest was not retrieved
                if (manifest == null)
                {
                    await RCFUI.MessageUI.DisplayMessageAsync(errorMessage, Resources.Update_ErrorHeader, MessageType.Error);
                    return;
                }

                // Flag indicating if the current update is a beta update
                bool isBetaUpdate = false;

                bool forceUpdates = RCFRCP.Data.ForceUpdate && isManualSearch;

                RCFCore.Logger?.LogInformationSource($"The update manifest was retrieved");

                try
                {
                    // Get the server version
                    var av = manifest["LatestAssemblyVersion"];
                    var serverVersion = new Version(av["Major"].Value<int>(), av["Minor"].Value<int>(), av["Build"].Value<int>(), av["Revision"].Value<int>());

                    // Compare version
                    if (RCFRCP.App.CurrentVersion >= serverVersion)
                    {
                        if (forceUpdates)
                        {
                            if (Data.GetBetaUpdates || IsBeta)
                                isBetaUpdate = true;
                        }
                        else
                        {
                            // Flag indicating if no new update is available
                            bool noUpdateAvailable = true;

                            // Get the beta version if checking for beta updates
                            if (Data.GetBetaUpdates || IsBeta)
                            {
                                // Get the beta version
                                var bv = manifest["LatestBetaVersion"];
                                var betaVersion = new Version(bv["Major"].Value<int>(), bv["Minor"].Value<int>(), bv["Build"].Value<int>(), bv["Revision"].Value<int>());

                                // Compare version
                                if (RCFRCP.App.CurrentVersion < betaVersion)
                                {
                                    isBetaUpdate = true;
                                    noUpdateAvailable = false;
                                }
                            }

                            if (noUpdateAvailable)
                            {
                                if (isManualSearch)
                                    await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, serverVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                                RCFCore.Logger?.LogInformationSource($"The latest version is installed");

                                return;
                            }
                        }
                    }

                    RCFCore.Logger?.LogInformationSource($"A new version ({serverVersion}) is available");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting assembly version from server manifest", manifest);
                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.Update_ManifestError, Resources.Update_ErrorHeader, MessageType.Error);
                    return;
                }

                string news = Resources.Update_NewsError;

                if (!isBetaUpdate)
                {
                    try
                    {
                        // Get the update news
                        news = manifest["DisplayNews"].Value<string>();
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Getting update news from server manifest", manifest);
                    }
                }

                if (await RCFUI.MessageUI.DisplayMessageAsync(!isBetaUpdate ? String.Format(Resources.Update_UpdateAvailable, news) : Resources.Update_BetaUpdateAvailable, Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                {
                    try
                    {
                        Directory.CreateDirectory(CommonPaths.UpdaterFilePath.Parent);
                        File.WriteAllBytes(CommonPaths.UpdaterFilePath, Files.Rayman_Control_Panel_Updater);
                        RCFCore.Logger?.LogInformationSource($"The updater was created");
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Writing updater to temp path", CommonPaths.UpdaterFilePath);
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_UpdaterError, "raycarrot.ylemnova.com"), Resources.Update_UpdaterErrorHeader, MessageType.Error);
                        return;
                    }

                    // Launch the updater and run as admin is set to show under installed programs in under to update the Registry key
                    if (await RCFRCP.File.LaunchFileAsync(CommonPaths.UpdaterFilePath, Data.ShowUnderInstalledPrograms, $"\"{Assembly.GetExecutingAssembly().Location}\" {RCFRCP.Data.DarkMode} {RCFRCP.Data.UserLevel} {isBetaUpdate} \"{Data.CurrentCulture}\"") == null)
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_RunningUpdaterError, "raycarrot.ylemnova.com"), Resources.Update_RunningUpdaterErrorHeader, MessageType.Error);

                        return;
                    }

                    // Shut down the app
                    Application.Current.Shutdown();
                }
            }
            finally
            {
                CheckingForUpdates = false;
            }
        }

        /// <summary>
        /// Attempts to move the backups from the old path to the new one
        /// </summary>
        /// <param name="oldPath">The old backup location</param>
        /// <param name="newPath">The new backup location</param>
        /// <returns>The task</returns>
        public async Task MoveBackupsAsync(FileSystemPath oldPath, FileSystemPath newPath)
        {
            using (await MoveBackupsAsyncLock.LockAsync())
            {
                if (!await RCFUI.MessageUI.DisplayMessageAsync(Resources.MoveBackups_Question, Resources.MoveBackups_QuestionHeader, MessageType.Question, true))
                {
                    RCFCore.Logger?.LogInformationSource("Moving old backups has been canceled by the user");
                    return;
                }

                try
                {
                    var oldLocation = oldPath + BackupFamily;
                    var newLocation = newPath + BackupFamily;

                    if (!oldLocation.DirectoryExists || !Directory.GetFileSystemEntries(oldLocation).Any())
                    {
                        RCFCore.Logger?.LogInformationSource("Old backups could not be moved due to not being found");

                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_NoBackupsFound, oldLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                        return;
                    }

                    if (newLocation.DirectoryExists)
                    {
                        RCFCore.Logger?.LogInformationSource("Old backups could not be moved due to the new location already existing");

                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_BackupAlreadyExists, newLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                        return;
                    }

                    RCFRCP.File.MoveDirectory(oldLocation, newLocation, false);

                    RCFCore.Logger?.LogInformationSource("Old backups have been moved");

                    await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, true, false));

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.MoveBackups_Success);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Moving backups");
                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.MoveBackups_Error, Resources.MoveBackups_ErrorHeader, MessageType.Error);
                }
            }
        }

        /// <summary>
        /// Opens the specified URL
        /// </summary>
        /// <param name="url">The URL to open</param>
        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(url)?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError($"Opening URL {url}");
            }
        }

        /// <summary>
        /// Restarts the Rayman Control Panel as administrator
        /// </summary>
        /// <returns>The task</returns>
        public async Task RestartAsAdminAsync()
        {
            await RunAdminWorkerAsync(AdminWorkerModes.RestartAsAdmin, Process.GetCurrentProcess().Id.ToString());
        }

        /// <summary>
        /// Runs the admin worker
        /// </summary>
        /// <param name="mode">The mode to run in</param>
        /// <param name="args">The mode arguments</param>
        /// <returns>The task</returns>
        public async Task RunAdminWorkerAsync(AdminWorkerModes mode, params string[] args)
        {
            using (await AdminWorkerAsyncLock.LockAsync())
                await RCFRCP.File.LaunchFileAsync(CommonPaths.AdminWorkerPath, true, $"{mode} {args.Select(x => $"\"{x}\"").JoinItems(" ")}");
        }

        /// <summary>
        /// Allows the user to locate the specified game and add it
        /// </summary>
        /// <param name="game">The game to locate</param>
        /// <returns>The task</returns>
        public async Task LocateGameAsync(Games game)
        {
            try
            {
                RCFCore.Logger?.LogTraceSource($"The game {game} is being located...");

                var typeResult = await game.GetGameTypeAsync();

                if (typeResult.CanceledByUser)
                    return;

                RCFCore.Logger?.LogInformationSource($"The game {game} type has been detected as {typeResult.SelectedType}");

                await game.GetGameManager(typeResult.SelectedType).LocateAddGameAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Locating game");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Deploys additional files, such as the uninstaller
        /// </summary>
        /// <returns>The task</returns>
        public async Task DeployFilesAsync(bool overwrite)
        {
            try
            {
                // Deploy the uninstaller
                if (overwrite || !CommonPaths.UninstallFilePath.FileExists)
                {
                    Directory.CreateDirectory(CommonPaths.UninstallFilePath.Parent);
                    File.WriteAllBytes(CommonPaths.UninstallFilePath, Files.Uninstaller);
                }

                // Deploy the admin worker
                if (overwrite || !CommonPaths.AdminWorkerPath.FileExists)
                {
                    Directory.CreateDirectory(CommonPaths.AdminWorkerPath.Parent);
                    File.WriteAllBytes(CommonPaths.AdminWorkerPath, Files.AdminWorker);
                }
            }
            catch (Exception ex)
            {
                ex.HandleCritical("Deploying additional files");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.DeployFilesError, MessageType.Error);
            }
        }

        /// <summary>
        /// Gets the existing backup location for the specified game if one exists
        /// </summary>
        /// <param name="compressedLocation">The location of the compressed backup file</param>
        /// <param name="normalLocation">The location of the normal backup directory</param>
        /// <returns>The backup location or null if none was found</returns>
        public FileSystemPath? GetExistingBackup(FileSystemPath compressedLocation, FileSystemPath normalLocation)
        {
            if (RCFRCP.Data.CompressBackups)
            {
                // Start by checking the location based on current setting
                if (compressedLocation.FileExists)
                    return compressedLocation;
                // Fall back to secondary location
                else if (normalLocation.DirectoryExists && Directory.GetFileSystemEntries(normalLocation).Any())
                    return normalLocation;
                else
                    // No valid location exists
                    return null;
            }
            else
            {
                // Start by checking the location based on current setting
                if (normalLocation.DirectoryExists && Directory.GetFileSystemEntries(normalLocation).Any())
                    return normalLocation;
                // Fall back to secondary location
                else if (compressedLocation.FileExists)
                    return compressedLocation;
                else
                    // No valid location exists
                    return null;
            }
        }

        /// <summary>
        /// Gets the backup file for the specified game if the backup is compressed
        /// </summary>
        /// <param name="backupName">The backup name</param>
        /// <returns>The backup file</returns>
        public FileSystemPath GetCompressedBackupFile(string backupName)
        {
            return RCFRCP.Data.BackupLocation + BackupFamily + (backupName + CommonPaths.BackupCompressionExtension);
        }

        /// <summary>
        /// Gets the backup directory for the specified game
        /// </summary>
        /// <param name="backupName">The backup name</param>
        /// <returns>The backup directory</returns>
        public FileSystemPath GetBackupDir(string backupName)
        {
            return RCFRCP.Data.BackupLocation + BackupFamily + backupName;
        }

        /// <summary>
        /// Requests the application to restart as administrator
        /// </summary>
        /// <returns>The task</returns>
        public async Task RequestRestartAsAdminAsync()
        {
            if (await RCFUI.MessageUI.DisplayMessageAsync(Resources.App_RequiresAdminQuestion, Resources.Restore_FailedHeader, MessageType.Warning, true))
                await RestartAsAdminAsync();
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
        /// Occurs when a refresh is required for the app
        /// </summary>
        public event AsyncEventHandler<RefreshRequiredEventArgs> RefreshRequired;

        #endregion
    }

    //public static class RCPHelpers
    //{
    //    private static Dictionary<Games, Dictionary<GameType, Type>> GameGenerator { get; } = new Dictionary<Games, Dictionary<GameType, Type>>()
    //    {
    //        {
    //            Games.Rayman2,
    //            new Dictionary<GameType, Type>()
    //            {
    //                {
    //                    GameType.Win32,
    //                    typeof(Rayman2_Win32)
    //                }
    //            }
    //        }
    //    };

    //    public static RCPGame GetGame(Games game, GameType type)
    //    {
    //        return GameGenerator[game][type].CreateInstance<RCPGame>();
    //    }
    //}
}