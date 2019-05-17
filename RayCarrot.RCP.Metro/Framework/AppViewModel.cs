using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ByteSizeLib;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            SaveUserDataAsyncLock = new AsyncLock();
            MoveBackupsAsyncLock = new AsyncLock();
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

        #region Public Properties

        /// <summary>
        /// A flag indicating if an update check is in progress
        /// </summary>
        public bool CheckingForUpdates { get; set; }

        /// <summary>
        /// An async lock for the <see cref="SaveUserDataAsync"/> method
        /// </summary>
        private AsyncLock SaveUserDataAsyncLock { get; }

        /// <summary>
        /// An async lock for the <see cref="MoveBackupsAsync"/> method
        /// </summary>
        private AsyncLock MoveBackupsAsyncLock { get; }

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentVersion => new Version(4, 3, 2, 0);

        /// <summary>
        /// Gets a collection of the available <see cref="Games"/>
        /// </summary>
        public IEnumerable<Games> GetGames => Enum.GetValues(typeof(Games)).Cast<Games>();

        /// <summary>
        /// Indicates if the game finder is currently running
        /// </summary>
        public bool IsGameFinderRunning { get; set; }

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

        #region Public Methods

        /// <summary>
        /// Fires the <see cref="RefreshRequired"/> event
        /// </summary>
        /// <param name="major">Indicates if a major change has occurred, such as a game having been removed or added</param>
        public void OnRefreshRequired(bool major)
        {
            RefreshRequired?.Invoke(this, new ValueEventArgs<bool>(major));
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
            RCF.Logger.LogInformationSource($"The game {game} is being added of type {type}...");

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                RCF.Logger.LogWarningSource($"The game {game} has already been added");

                await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, game), Resources.AddGame_DuplicateHeader, MessageType.Error);

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
                    try
                    {
                        installDirectory = game.GetPackageInstallDirectory();
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Getting Windows Store game install directory");
                    }
                }
                else
                {
                    RCF.Logger.LogErrorSource($"The game {game} can not be added due to the install directory being null");
                    return;
                }

                RCF.Logger.LogInformationSource($"The game {game} install directory was retrieved as {installDirectory}");
            }

            // Default save data location based on version
            if (game == Games.RaymanRavingRabbids)
                Data.RRRIsSaveDataInInstallDir = type == GameType.Steam;

            // Add the game
            Data.Games.Add(game, new GameInfo(type, installDirectory ?? FileSystemPath.EmptyPath));

            // If it's a DosBox game, create the auto config file
            if (type == GameType.DosBox)
                // Create config file
                new DosBoxAutoConfigManager(game.GetDosBoxConfigFile()).Create();

            // If it's a DosBox game, add the DosBox options
            if (type == GameType.DosBox && !Data.DosBoxGames.ContainsKey(game))
                Data.DosBoxGames.Add(game, new DosBoxOptions());

            RCF.Logger.LogInformationSource($"The game {game} has been added");

            // Refresh
            OnRefreshRequired(true);
        }

        /// <summary>
        /// Removes the specified game
        /// </summary>
        /// <param name="game">The game to remove</param>
        /// <param name="forceRemove">Indicates if the game should be force removed</param>
        /// <returns>The task</returns>
        public async Task RemoveGameAsync(Games game, bool forceRemove)
        {
            if (!forceRemove)
            {
                // Get applied utilities
                var utilities = await game.GetAppliedUtilitiesAsync();

                // Warn about utilities
                if (utilities.Any() && !await RCF.MessageUI.DisplayMessageAsync(
                        $"{Resources.RemoveGame_UtilityWarning}{Environment.NewLine}{Environment.NewLine}{utilities.JoinItems(Environment.NewLine)}", Resources.RemoveGame_UtilityWarningHeader, MessageType.Warning, true))
                    return;
            }

            // Remove the game
            Data.Games.Remove(game);

            // If there is DosBox options saved, remove those as well
            if (Data.DosBoxGames.ContainsKey(game))
            {
                Data.DosBoxGames.Remove(game);

                try
                {
                    // Remove the config file
                    RCFRCP.File.DeleteFile(game.GetDosBoxConfigFile());
                }
                catch (Exception ex)
                {
                    ex.HandleError("Removing DosBox auto config file");
                }
            }

            // Refresh the games
            OnRefreshRequired(true);
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
        /// Enables write access to the primary ubi.ini file
        /// </summary>
        /// <returns>The task</returns>
        public async Task EnableUbiIniWriteAccessAsync()
        {
            try
            {
                if (!CommonPaths.UbiIniPath1.FileExists)
                {
                    RCF.Logger.LogInformationSource("The ubi.ini file was not found");
                    return;
                }

                // Check if we have write access
                if (RCFRCP.File.CheckFileWriteAccess(CommonPaths.UbiIniPath1))
                {
                    RCF.Logger.LogDebugSource("The ubi.ini file has write access");
                    return;
                }

                await RCF.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

                // Attempt to change the permission through CMD to avoid having to run RCP as admin
                // ReSharper disable once StringLiteralTypo
                WindowsHelpers.RunCommandPromptScript($"icacls \"{CommonPaths.UbiIniPath1}\" /grant Users:W", true);

                RCF.Logger.LogInformationSource($"The ubi.ini file permission was changed");
            }
            catch (Exception ex)
            {
                ex.HandleError("Changing ubi.ini file permissions");
                await RCF.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_Error, MessageType.Error);
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

            return await Task.Run(async () =>
            {
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

                                RCF.Logger.LogTraceSource($"The DosBox executable was found from the game checker with the source {dosBoxCheckResult.Source}");

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
                        if (game.IsValid(GameType.WinStore, FileSystemPath.EmptyPath))
                        {
                            result.Add(game);

                            // Add the game
                            await AddNewGameAsync(game, GameType.WinStore);

                            RCF.Logger.LogInformationSource($"The game {game.GetDisplayName()} has been added from the game finder");
                        }
                    }

                    // Check Windows Store apps
                    if (!Games.RaymanJungleRun.IsAdded())
                        await FindWinStoreAppAsync(Games.RaymanJungleRun);

                    if (!Games.RaymanFiestaRun.IsAdded())
                    {
                        Data.IsFiestaRunWin10Edition = true;

                        await FindWinStoreAppAsync(Games.RaymanFiestaRun);
                    }

                    if (!Games.RaymanFiestaRun.IsAdded())
                    {
                        Data.IsFiestaRunWin10Edition = false;

                        await FindWinStoreAppAsync(Games.RaymanFiestaRun);
                    }

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

                            if (!File.Exists(Data.DosBoxPath))
                                Data.DosBoxPath = Path.Combine(dir, "DosBox\\DOSBox.exe");

                            Data.DosBoxConfig = Path.Combine(dir, "dosboxRayman.conf");

                            // Add the games
                            await AddNewGameAsync(Games.Rayman1, GameType.DosBox, dir + "Rayman");
                            await AddNewGameAsync(Games.RaymanDesigner, GameType.DosBox, dir + "RayKit");
                            await AddNewGameAsync(Games.RaymanByHisFans, GameType.DosBox, dir + "RayFan");

                            RCF.Logger.LogInformationSource($"The games in Rayman Forever has been added from the game finder");

                            var mountPath = mountFileA.FileExists ? mountFileA : mountFileB;

                            Data.DosBoxGames[Games.Rayman1].MountPath = mountPath;
                            Data.DosBoxGames[Games.RaymanDesigner].MountPath = mountPath;
                            Data.DosBoxGames[Games.RaymanByHisFans].MountPath = mountPath;
                        }
                    }

                    if (result.Count > 0)
                    {
                        await RCF.MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {result.JoinItems(Environment.NewLine + "• ", x => x.GetDisplayName())}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);
                        RCF.Logger.LogInformationSource($"The game finder found the following games {result.JoinItems(", ")}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Game finder");
                    await RCF.MessageUI.DisplayMessageAsync(Resources.GameFinder_Error, MessageType.Error);
                }
                finally
                {
                    await SaveUserDataAsync();
                    IsGameFinderRunning = false;
                }

                return false;
            });
        }

        /// <summary>
        /// Cleans the temporary files of the application
        /// </summary>
        public void CleanTemp()
        {
            lock (this)
            {
                try
                {
                    RCFRCP.File.DeleteDirectory(CommonPaths.TempPath);

                    RCF.Logger.LogInformationSource($"The application temp was cleaned");
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Cleaning temp");
                }
            }
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
                RCF.Logger.LogInformationSource($"A download is starting...");

                // Make sure the directory exists
                if (!outputDir.DirectoryExists)
                    Directory.CreateDirectory(outputDir);

                // Make sure there are input sources to download
                if (!inputSources.Any())
                {
                    RCF.Logger.LogInformationSource($"Download failed due to there not being any input sources");

                    await RCF.MessageUI.DisplayMessageAsync(Resources.Download_NoFilesFound, MessageType.Error);
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

                    RCF.Logger.LogDebugSource($"The size of the download has been retrieved as {size}");

                    if (!await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Download_ConfirmSize, size), Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting download size");
                    if (!await RCF.MessageUI.DisplayMessageAsync(Resources.Download_Confirm, Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }

                // Create the download dialog
                var dialog = new Downloader(isCompressed ? new DownloaderViewModel(inputSources.First(), outputDir) : new DownloaderViewModel(inputSources, outputDir));

                // Show the dialog
                dialog.ShowDialog();

                RCF.Logger.LogInformationSource($"The download finished with the result of {dialog.ViewModel.DownloadState}");

                // Return the result
                return dialog.ViewModel.DownloadState == DownloadState.Succeeded;
            }
            catch (Exception ex)
            {
                ex.HandleError($"Downloading files");
                await RCF.MessageUI.DisplayMessageAsync(Resources.Download_Error, MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Checks for application updates
        /// </summary>
        /// <returns>The task</returns>
        public async Task CheckForUpdatesAsync(bool showIfNoUpdates)
        {
            if (CheckingForUpdates)
                return;

            try
            {
                RCF.Logger.LogInformationSource($"Updates are being checked for");

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
                    await RCF.MessageUI.DisplayMessageAsync(errorMessage, Resources.Update_ErrorHeader, MessageType.Error);
                    return;
                }

                RCF.Logger.LogInformationSource($"The update manifest was retrieved");

                try
                {
                    // Get the server version
                    var av = manifest["LatestAssemblyVersion"];
                    var serverVersion = new Version(av["Major"].Value<int>(), av["Minor"].Value<int>(), av["Build"].Value<int>(), av["Revision"].Value<int>());

                    // Compare version
                    if (RCFRCP.App.CurrentVersion >= serverVersion)
                    {
                        if (showIfNoUpdates)
                            await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, serverVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                        RCF.Logger.LogInformationSource($"The latest version is installed");

                        return;
                    }

                    RCF.Logger.LogInformationSource($"A new version ({serverVersion}) is available");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting assembly version from server manifest", manifest);
                    await RCF.MessageUI.DisplayMessageAsync(Resources.Update_ManifestError, Resources.Update_ErrorHeader, MessageType.Error);
                    return;
                }

                string news = Resources.Update_NewsError;
                try
                {
                    // Get the update news
                    news = manifest["DisplayNews"].Value<string>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting update news from server manifest", manifest);
                }

                if (await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_UpdateAvailable, news), Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                {
                    string path = Path.Combine(Path.GetTempPath(), "RCP_Updater.exe");

                    try
                    {
                        File.WriteAllBytes(path, Files.Rayman_Control_Panel_Updater);
                        RCF.Logger.LogInformationSource($"The updater was created");
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Writing updater to temp path", path);
                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_UpdaterError, "raycarrot.ylemnova.com"), Resources.Update_UpdaterErrorHeader, MessageType.Error);
                        return;
                    }

                    await RCFRCP.File.LaunchFileAsync(path, false, $"\"{Assembly.GetExecutingAssembly().Location}\" {RCFRCP.Data.UserLevel} True");
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
                if (!await RCF.MessageUI.DisplayMessageAsync(Resources.MoveBackups_Question, Resources.MoveBackups_QuestionHeader, MessageType.Question, true))
                {
                    RCF.Logger.LogInformationSource("Moving old backups has been canceled by the user");
                    return;
                }

                try
                {
                    var oldLocation = oldPath + BackupFamily;
                    var newLocation = newPath + BackupFamily;

                    if (!oldLocation.DirectoryExists || !Directory.GetFileSystemEntries(oldLocation).Any())
                    {
                        RCF.Logger.LogInformationSource("Old backups could not be moved due to not being found");

                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_NoBackupsFound, oldLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                        return;
                    }

                    if (newLocation.DirectoryExists)
                    {
                        RCF.Logger.LogInformationSource("Old backups could not be moved due to the new location already existing");

                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_BackupAlreadyExists, newLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                        return;
                    }

                    RCFRCP.File.MoveDirectory(oldLocation, newLocation, false);

                    RCF.Logger.LogInformationSource("Old backups have been moved");

                    await RCF.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.MoveBackups_Success);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Moving backups");
                    await RCF.MessageUI.DisplayMessageAsync(Resources.MoveBackups_Error, Resources.MoveBackups_ErrorHeader, MessageType.Error);
                }
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
        public event EventHandler<ValueEventArgs<bool>> RefreshRequired;

        #endregion
    }
}