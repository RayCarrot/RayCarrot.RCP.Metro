﻿using ByteSizeLib;
using Newtonsoft.Json;
using Nito.AsyncEx;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles common actions and events for this application
    /// </summary>
    public class AppViewModel : BaseViewModel
    {
        #region Static Constructor

        static AppViewModel()
        {
            // Get the current Window version
            WindowsVersion = WindowsHelpers.GetCurrentWindowsVersion();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>'
        /// <param name="startupLogAction">Optional startup logging action</param>
        public AppViewModel(Action<string> startupLogAction = null)
        {
            startupLogAction?.Invoke("AppVM: Creating app view model");

            // Flag that the startup has begun
            IsStartupRunning = true;

            // Check if the application is running as administrator
            try
            {
                startupLogAction?.Invoke("AppVM: Checking if running as admin");

                IsRunningAsAdmin = WindowsHelpers.RunningAsAdmin;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                IsRunningAsAdmin = false;
            }

            startupLogAction?.Invoke("AppVM: Creating locks & commands");

            // Create locks
            SaveUserDataAsyncLock = new AsyncLock();
            MoveBackupsAsyncLock = new AsyncLock();
            AdminWorkerAsyncLock = new AsyncLock();
            OnRefreshRequiredAsyncLock = new AsyncLock();

            // Create commands
            RestartAsAdminCommand = new AsyncRelayCommand(RestartAsAdminAsync);
            RequestRestartAsAdminCommand = new AsyncRelayCommand(RequestRestartAsAdminAsync);

            startupLogAction?.Invoke("AppVM: Reading game config");

            // Read the game manager configuration
            var gameConfig = Files.Games;

            // Set up the games manager
            GamesManager = JsonConvert.DeserializeObject<AppGamesManager>(gameConfig, new SimpleTypeConverter());

            startupLogAction?.Invoke("AppVM: Finished reading game config");
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Commands

        public ICommand RestartAsAdminCommand { get; }

        public ICommand RequestRestartAsAdminCommand { get; }

        #endregion

        #region Constant Fields

        /// <summary>
        /// The application base path to use for WPF related operations
        /// </summary>
        public const string WPFApplicationBasePath = "pack://application:,,,/RayCarrot.RCP.Metro;component/";

        /// <summary>
        /// The name of the backup directory for this application
        /// </summary>
        public const string BackupFamily = "Rayman Game Backups";

        #endregion

        #region Private Fields

        private AppPage _selectedPage;

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

        /// <summary>
        /// Indicates if a serializer logger has been created during the app life-cycle
        /// </summary>
        private bool HasCreatedSerializerLogger { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentAppVersion => new Version(12, 0, 0, 5);

        /// <summary>
        /// Indicates if the current version is a beta version
        /// </summary>
        public bool IsBeta => true;

        /// <summary>
        /// Shortcut to the app user data
        /// </summary>
        public AppUserData Data => Services.Data;

        /// <summary>
        /// The application games manager
        /// </summary>
        public AppGamesManager GamesManager { get; }

        /// <summary>
        /// Gets a collection of the available <see cref="Games"/>
        /// </summary>
        public IEnumerable<Games> GetGames => Enum.GetValues(typeof(Games)).Cast<Games>();

        /// <summary>
        /// Gets a collection of the available <see cref="Games"/> categorized
        /// </summary>
        public Dictionary<GameCategory, Games[]> GetCategorizedGames =>
            GetGames
                // Group the games by the category
                .GroupBy(x => x.GetGameInfo().Category).
                // Create a dictionary
                ToDictionary(x => x.Key, y => y.ToArray());

        /// <summary>
        /// Indicates if the game finder is currently running
        /// </summary>
        public bool IsGameFinderRunning { get; set; }

        /// <summary>
        /// The currently selected page
        /// </summary>
        public AppPage SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (_selectedPage == value)
                    return;

                var oldValue = _selectedPage;
                _selectedPage = value;
                OnSelectedPageChanged(new PropertyChangedEventArgs<AppPage>(oldValue, value));
            }
        }

        /// <summary>
        /// A flag indicating if an update check is in progress
        /// </summary>
        public bool CheckingForUpdates { get; set; }

        /// <summary>
        /// Indicates if the application is running as administrator
        /// </summary>
        public bool IsRunningAsAdmin { get; }

        /// <summary>
        /// Indicates if the application startup is running
        /// </summary>
        public bool IsStartupRunning { get; set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The Windows version the program is running on
        /// </summary>
        public static WindowsVersion WindowsVersion { get; }

        #endregion

        #region Protected Methods

        protected virtual void OnSelectedPageChanged(PropertyChangedEventArgs<AppPage> e)
        {
            SelectedPageChanged?.Invoke(this, e);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the binary serializer logger to use
        /// </summary>
        /// <param name="name">An optional name for the logging session</param>
        /// <param name="addr">An optional address for the logging session</param>
        /// <returns>The binary serializer logger</returns>
        public IBinarySerializerLogger GetBinarySerializerLogger(string name = null, long? addr = null)
        {
            if (Services.Data.Binary_BinarySerializationFileLogPath.FullPath.IsNullOrWhiteSpace() ||
                !Services.Data.Binary_BinarySerializationFileLogPath.Parent.DirectoryExists)
                return null;

            // Get the log file path
            var logPath = Services.Data.Binary_BinarySerializationFileLogPath;

            // Only re-create the file if this is the first time we create a logger
            Stream logStream = !HasCreatedSerializerLogger ? File.Create(logPath) : File.OpenWrite(logPath);

            logStream.Seek(0, SeekOrigin.End);

            HasCreatedSerializerLogger = true;

            var logger = new BinarySerializerFileLogger(logStream);

            if (logStream.Length > 0)
                logger.WriteLogLine(String.Empty);

            logger.WriteLogLine($"=== Serializing{(name != null ? $" {name}" : String.Empty)}{(addr != null ? $" at 0x{addr:X8}" : String.Empty)} ===");

            return logger;
        }

        /// <summary>
        /// Gets new instances of utilities for a specific game
        /// </summary>
        /// <param name="game">The game to get the utilities for</param>
        /// <returns>The utilities instances</returns>
        public IEnumerable<IUtility> GetUtilities(Games game)
        {
            var utilities = GamesManager.LocalUtilities.TryGetValue(game);

            if (utilities == null)
                return new IUtility[0]; 

            return utilities.
                // Create a new instance of each utility
                Select(x => x.CreateInstance<IUtility>()).
                // Make sure it's available
                Where(x => x.IsAvailable);
        }

        /// <summary>
        /// Fires the <see cref="RefreshRequired"/> event
        /// </summary>
        /// <returns>The task</returns>
        public async Task OnRefreshRequiredAsync(RefreshRequiredEventArgs eventArgs)
        {
            // Lock the refresh
            using (await OnRefreshRequiredAsyncLock.LockAsync())
            {
                Logger.Debug("A refresh is being requested");

                // Await the refresh event
                await (RefreshRequired?.RaiseAsync(this, eventArgs) ?? Task.CompletedTask);
            }
        }

        /// <summary>
        /// Adds a new game to the app data
        /// </summary>
        /// <param name="game">The game to add</param>
        /// <param name="type">The game type</param>
        /// <param name="installDirectory">The game install directory</param>
        /// <returns>The task</returns>
        public async Task AddNewGameAsync(Games game, GameType type, FileSystemPath installDirectory)
        {
            Logger.Info("The game {0} is being added of type {1}...", game, type);

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                Logger.Warn("The game {0} has already been added", game);

                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, game), Resources.AddGame_DuplicateHeader, MessageType.Error);

                return;
            }

            // Get the manager
            var manager = game.GetManager(type);

            // Add the game
            Data.Game_Games.Add(game, new UserData_GameData(type, installDirectory));

            Logger.Info("The game {0} has been added", game);

            // Run post-add operations
            await manager.PostGameAddAsync();

            // Add the game to the jump list
            Data.App_JumpListItemIDCollection.AddRange(manager.GetJumpListItems().Select(x => x.ID));
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
                // Get the manager
                var manager = game.GetManager();

                if (!forceRemove)
                {
                    // Get applied utilities
                    var appliedUtilities = await game.GetGameInfo().GetAppliedUtilitiesAsync();

                    // Warn about applied utilities, if any
                    if (appliedUtilities.Any() && !await Services.MessageUI.DisplayMessageAsync($"{Resources.RemoveGame_UtilityWarning}{Environment.NewLine}{Environment.NewLine}{appliedUtilities.JoinItems(Environment.NewLine)}", Resources.RemoveGame_UtilityWarningHeader, MessageType.Warning, true))
                        return;
                }

                // Remove the game from the jump list
                foreach (var item in manager.GetJumpListItems())
                    Data.App_JumpListItemIDCollection?.RemoveWhere(x => x == item.ID);

                // Remove game from installed games
                if (Data.Game_InstalledGames.Contains(game))
                    Data.Game_InstalledGames.Remove(game);

                // Remove the game
                Data.Game_Games.Remove(game);

                // Run post game removal
                await manager.PostGameRemovedAsync();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Removing game");
                throw;
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
                if (!AppFilePaths.UbiIniPath1.FileExists)
                {
                    Logger.Info("The ubi.ini file was not found");
                    return;
                }

                // Check if we have write access
                if (Services.File.CheckFileWriteAccess(AppFilePaths.UbiIniPath1))
                {
                    Logger.Debug("The ubi.ini file has write access");
                    return;
                }

                await Services.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

                // Attempt to change the permission
                await RunAdminWorkerAsync(AdminWorkerMode.GrantFullControl, true, AppFilePaths.UbiIniPath1);

                Logger.Info("The ubi.ini file permission was changed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Changing ubi.ini file permissions");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.UbiIniWriteAccess_Error);
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

            // Keep track of found games which have been added
            var addedGames = new List<Games>();

            try
            {
                // Get all games which have not been added
                var games = GetGames.Where(x => !x.IsAdded()).ToArray();

                Logger.Trace("The following games were added to the game checker: {0}", games.JoinItems(", "));

                // Get additional finder items
                var finderItems = new List<GameFinder_GenericItem>(1);

                // Create DOSBox finder item if it doesn't exist
                if (!File.Exists(Data.Emu_DOSBox_Path))
                {
                    var names = new string[]
                    {
                        "DosBox",
                        "Dos Box"
                    };

                    void foundAction(FileSystemPath installDir, object parameter)
                    {
                        if (File.Exists(Data.Emu_DOSBox_Path))
                        {
                            Logger.Warn("The DosBox executable was not added from the game finder due to already having been added");
                            return;
                        }

                        Logger.Info("The DosBox executable was found from the game finder");

                        Data.Emu_DOSBox_Path = installDir + "DOSBox.exe";
                    }

                    finderItems.Add(new GameFinder_GenericItem(names, "DosBox", x => (x + "DOSBox.exe").FileExists ? x : null, foundAction, "DOSBox"));
                }

                // Run the game finder and get the result
                var foundItems = await new GameFinder(games, finderItems).FindGamesAsync();

                // Add the found items
                foreach (var foundItem in foundItems)
                {
                    // Handle the item
                    await foundItem.HandleItemAsync();

                    // If a game, add to list
                    if (foundItem is GameFinder_GameResult game)
                        addedGames.Add(game.Game);
                }

                // Show message if new games were found
                if (foundItems.Count > 0)
                {
                    // Split into found games and items and sort
                    var gameFinderResults = foundItems.OfType<GameFinder_GameResult>().OrderBy(x => x.Game).Select(x => x.DisplayName);
                    var finderResults = foundItems.OfType<GameFinder_GenericResult>().OrderBy(x => x.DisplayName).Select(x => x.DisplayName);

                    await Services.MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {gameFinderResults.Concat(finderResults).JoinItems(Environment.NewLine + "• ")}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);

                    Logger.Info("The game finder found the following games {0}", foundItems.JoinItems(", ", x => x.ToString()));

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Game finder");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameFinder_Error);
            }
            finally
            {
                // Refresh if any games were added
                if (addedGames.Any())
                    await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(addedGames, RefreshFlags.GameCollection));

                IsGameFinderRunning = false;
            }

            return false;
        }

        /// <summary>
        /// Downloads the specified files to a specified output directory
        /// </summary>
        /// <param name="inputSources">The files to download</param>
        /// <param name="isCompressed">True if the download is a compressed file, otherwise false</param>
        /// <param name="outputDir">The output directory to download to</param>
        /// <param name="isGame">Indicates if the download is for a game. If false it is assumed to be a generic patch.</param>
        /// <returns>True if the download succeeded, otherwise false</returns>
        public async Task<bool> DownloadAsync(IList<Uri> inputSources, bool isCompressed, FileSystemPath outputDir, bool isGame = false)
        {
            try
            {
                Logger.Info("A download is starting...");

                // Make sure there are input sources to download
                if (!inputSources.Any())
                {
                    Logger.Info("Download failed due to there not being any input sources");

                    await Services.MessageUI.DisplayMessageAsync(Resources.Download_NoFilesFound, MessageType.Error);
                    return false;
                }

                if (Data.App_HandleDownloadsManually)
                {
                    var result = await Services.UI.DisplayMessageAsync(Resources.Download_ManualInstructions, Resources.Download_ManualHeader, MessageType.Information, true, new DialogMessageActionViewModel[]
                        {
                            // Download files
                            new DialogMessageActionViewModel
                            {
                                DisplayText = Resources.Download_ManualDownload,
                                ShouldCloseDialog = false,
                                OnHandled = () =>
                                {
                                    foreach (var u in inputSources)
                                        OpenUrl(u.AbsoluteUri);
                                }
                            },
                            // Open destination folder
                            new DialogMessageActionViewModel
                            {
                                DisplayText = Resources.Download_ManualOpenDestination,
                                ShouldCloseDialog = false,
                                OnHandled = () =>
                                {
                                    Directory.CreateDirectory(outputDir);
                                    Task.Run(async () => await Services.File.OpenExplorerLocationAsync(outputDir));
                                }
                            },
                        });

                    if (!result)
                        return false;

                    Logger.Info("Manual download finished");

                    // Return the result
                    return true;
                }
                else
                {
                    // Allow user to confirm
                    try
                    {
                        ByteSize size = ByteSize.FromBytes(0);
                        foreach (var item in inputSources)
                        {
                            var webRequest = WebRequest.Create(item);
                            webRequest.Method = "HEAD";

                            using var webResponse = webRequest.GetResponse();
                            size = size.Add(ByteSize.FromBytes(Convert.ToDouble(webResponse.Headers.Get("Content-Length"))));
                        }

                        Logger.Debug("The size of the download has been retrieved as {0}", size);

                        if (!await Services.MessageUI.DisplayMessageAsync(String.Format(isGame ? Resources.DownloadGame_ConfirmSize : Resources.Download_ConfirmSize, size), Resources.Download_ConfirmHeader, MessageType.Question, true))
                            return false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Getting download size");
                        if (!await Services.MessageUI.DisplayMessageAsync(isGame ? Resources.DownloadGame_Confirm : Resources.Download_Confirm, Resources.Download_ConfirmHeader, MessageType.Question, true))
                            return false;
                    }
                }

                // Show the downloader and get the result
                var dialogResult = await Services.UI.DownloadAsync(new DownloaderViewModel(inputSources, outputDir, isCompressed));

                Logger.Info("The download finished with the result of {0}", dialogResult.DownloadState);

                // Return the result
                return dialogResult.DownloadState == DownloaderViewModel.DownloadState.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Downloading files");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Download_Error);
                return false;
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
            // Lock moving the backups
            using (await MoveBackupsAsyncLock.LockAsync())
            {
                if (!await Services.MessageUI.DisplayMessageAsync(Resources.MoveBackups_Question, Resources.MoveBackups_QuestionHeader, MessageType.Question, true))
                {
                    Logger.Info("Moving old backups has been canceled by the user");
                    return;
                }

                try
                {
                    // Get the complete paths
                    var oldLocation = oldPath + BackupFamily;
                    var newLocation = newPath + BackupFamily;

                    // Make sure the old location has backups
                    if (!oldLocation.DirectoryExists || !Directory.GetFileSystemEntries(oldLocation).Any())
                    {
                        Logger.Info("Old backups could not be moved due to not being found");

                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_NoBackupsFound, oldLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);

                        return;
                    }

                    // Make sure the new location doesn't already exist
                    if (newLocation.DirectoryExists)
                    {
                        Logger.Info("Old backups could not be moved due to the new location already existing");

                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_BackupAlreadyExists, newLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                        return;
                    }

                    // Move the directory
                    Services.File.MoveDirectory(oldLocation, newLocation, false, false);

                    Logger.Info("Old backups have been moved");

                    // Refresh backups
                    await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.Backups));

                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.MoveBackups_Success);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Moving backups");
                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.MoveBackups_Error, Resources.MoveBackups_ErrorHeader);
                }
            }
        }

        /// <summary>
        /// Restarts the Rayman Control Panel as administrator
        /// </summary>
        /// <returns>The task</returns>
        public async Task RestartAsAdminAsync()
        {
            // Run the admin worker, setting it to restart this process as admin
            await RunAdminWorkerAsync(AdminWorkerMode.RestartAsAdmin, true, Process.GetCurrentProcess().Id.ToString());
        }

        /// <summary>
        /// Restarts the Rayman Control Panel with the specified arguments
        /// </summary>
        /// <returns>The task</returns>
        public async Task RestartAsync(params string[] args)
        {
            // Run the admin worker, setting it to restart this process as admin
            await RunAdminWorkerAsync(AdminWorkerMode.RestartWithArgs, false, new string[]
            {
                Process.GetCurrentProcess().Id.ToString()
            }.Concat(args).ToArray());
        }

        /// <summary>
        /// Runs the admin worker
        /// </summary>
        /// <param name="mode">The mode to run in</param>
        /// <param name="asAdmin">Indicates if the admin worker should be run with admin privileges</param>
        /// <param name="args">The mode arguments</param>
        /// <returns>The task</returns>
        public async Task RunAdminWorkerAsync(AdminWorkerMode mode, bool asAdmin, params string[] args)
        {
            // Lock
            using (await AdminWorkerAsyncLock.LockAsync())
                // Launch the admin worker with the specified launch arguments
                await Services.File.LaunchFileAsync(AppFilePaths.AdminWorkerPath, asAdmin, $"{mode} {args.Select(x => $"\"{x}\"").JoinItems(" ")}");
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
                if (overwrite || !AppFilePaths.UninstallFilePath.FileExists)
                {
                    Directory.CreateDirectory(AppFilePaths.UninstallFilePath.Parent);
                    File.WriteAllBytes(AppFilePaths.UninstallFilePath, Files.Uninstaller);
                }

                // Deploy the admin worker
                if (overwrite || !AppFilePaths.AdminWorkerPath.FileExists)
                {
                    Directory.CreateDirectory(AppFilePaths.AdminWorkerPath.Parent);
                    File.WriteAllBytes(AppFilePaths.AdminWorkerPath, Files.AdminWorker);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Deploying additional files");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.DeployFilesError);
            }
        }

        /// <summary>
        /// Requests the application to restart as administrator
        /// </summary>
        /// <returns>The task</returns>
        public async Task RequestRestartAsAdminAsync()
        {
            // Request restarting the application as admin
            if (await Services.MessageUI.DisplayMessageAsync(Resources.App_RequiresAdminQuestion, Resources.App_RestartAsAdmin, MessageType.Warning, true))
                // Restart as admin
                await RestartAsAdminAsync();
        }

        /// <summary>
        /// Opens the specified URL
        /// </summary>
        /// <param name="url">The URL to open</param>
        public void OpenUrl(string url)
        {
            try
            {
                // Open the URL in default browser
                Process.Start(url)?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Opening URL {0}", url);
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
                CheckingForUpdates = true;

                // Check for updates
                var result = await Services.UpdaterManager.CheckAsync(Services.Data.Update_ForceUpdate && isManualSearch, Services.Data.Update_GetBetaUpdates || IsBeta);

                // Check if there is an error
                if (result.ErrorMessage != null)
                {
                    await Services.MessageUI.DisplayExceptionMessageAsync(result.Exception,
                        String.Format(Resources.Update_CheckFailed, result.ErrorMessage, AppURLs.RCPBaseUrl), Resources.Update_ErrorHeader);

                    Services.Data.Update_IsUpdateAvailable = false;

                    return;
                }

                // Check if no new updates were found
                if (!result.IsNewUpdateAvailable)
                {
                    if (isManualSearch)
                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, CurrentAppVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                    Services.Data.Update_IsUpdateAvailable = false;

                    return;
                }

                // Indicate that a new update is available
                Services.Data.Update_IsUpdateAvailable = true;

                // Run as new task to mark this operation as finished
                Task.Run(async () =>
                {
                    try
                    {
                        var isBeta = result.IsBetaUpdate;
                        var message = String.Format(!isBeta 
                            ? Resources.Update_UpdateAvailable 
                            : Resources.Update_BetaUpdateAvailable, result.DisplayNews);

                        if (await Services.MessageUI.DisplayMessageAsync(message, Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                        {
                            // Launch the updater and run as admin if set to show under installed programs in under to update the Registry key
                            var succeeded = await Services.UpdaterManager.UpdateAsync(result, false);

                            if (!succeeded)
                                Logger.Warn("The updater failed to update the program");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Updating RCP");
                        await Services.MessageUI.DisplayMessageAsync(Resources.Update_Error, Resources.Update_ErrorHeader, MessageType.Error);
                    }
                }).WithoutAwait("Updating");
            }
            finally
            {
                CheckingForUpdates = false;
            }
        }

        /// <summary>
        /// Saves all user data for the application
        /// </summary>
        public virtual async Task SaveUserDataAsync()
        {
            // Lock the saving of user data
            using (await SaveUserDataAsyncLock.LockAsync())
            {
                // Run it as a new task
                await Task.Run(() =>
                {
                    // Save the user data
                    try
                    {
                        // Save the user data
                        JsonHelpers.SerializeToFile(Services.Data, AppFilePaths.AppUserDataPath);

                        Logger.Info("The application user data was saved");
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex, "Saving user data");
                    }
                });
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a refresh is required for the app
        /// </summary>
        public event AsyncEventHandler<RefreshRequiredEventArgs> RefreshRequired;

        /// <summary>
        /// Occurs when the selected page changes
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs<AppPage>> SelectedPageChanged;

        #endregion

        #region Classes

        /// <summary>
        /// The application game manager
        /// </summary>
        public class AppGamesManager
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="localUtilities">The available local utilities</param>
            /// <param name="gameManagers">The available game managers</param>
            /// <param name="gameInfos">The available game infos</param>
            public AppGamesManager(Dictionary<Games, Type[]> localUtilities, Dictionary<Games, Dictionary<GameType, Type>> gameManagers, Dictionary<Games, Type> gameInfos)
            {
                LocalUtilities = localUtilities;
                GameManagers = gameManagers;
                GameInfos = gameInfos;
                InstanceCache = new Dictionary<Type, object>();
            }

            /// <summary>
            /// The available local utilities
            /// </summary>
            public Dictionary<Games, Type[]> LocalUtilities { get; }

            /// <summary>
            /// The available game managers
            /// </summary>
            public Dictionary<Games, Dictionary<GameType, Type>> GameManagers { get; }

            /// <summary>
            /// The available game infos
            /// </summary>
            public Dictionary<Games, Type> GameInfos { get; }

            /// <summary>
            /// Creates a new instance of the specified type or gets an existing one from cache
            /// </summary>
            /// <typeparam name="T">The object type to cast to</typeparam>
            /// <param name="t">The object type to create from</param>
            /// <returns>The object</returns>
            public T CreateCachedInstance<T>(Type t)
            {
                if (InstanceCache.ContainsKey(t))
                    return (T)InstanceCache[t];

                var obj = t.CreateInstance<T>();
                InstanceCache[t] = obj;
                return obj;
            }

            private Dictionary<Type, object> InstanceCache { get; }
        }

        #endregion
    }
}