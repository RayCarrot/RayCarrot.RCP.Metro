using ByteSizeLib;
using Newtonsoft.Json;
using Nito.AsyncEx;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RayCarrot.Binary;
using RayCarrot.Logging;
using RayCarrot.WPF;

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
        /// </summary>
        public AppViewModel()
        {
            // Flag that the startup has begun
            IsStartupRunning = true;

            // Check if the application is running as administrator
            try
            {
                IsRunningAsAdmin = WindowsHelpers.RunningAsAdmin;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                IsRunningAsAdmin = false;
            }

            // Create locks
            SaveUserDataAsyncLock = new AsyncLock();
            MoveBackupsAsyncLock = new AsyncLock();
            AdminWorkerAsyncLock = new AsyncLock();
            OnRefreshRequiredAsyncLock = new AsyncLock();

            // Create commands
            RestartAsAdminCommand = new AsyncRelayCommand(RestartAsAdminAsync);
            RequestRestartAsAdminCommand = new AsyncRelayCommand(RequestRestartAsAdminAsync);

            // Read the game manager configuration
            var gameConfig = Files.Games;

            // Set up the games manager
            GamesManager = JsonConvert.DeserializeObject<AppGamesManager>(gameConfig, new SimpleTypeConverter());
        }

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
        /// The current app version
        /// </summary>
        public Version CurrentAppVersion => new Version(10, 1, 0, 3);

        /// <summary>
        /// Indicates if the current version is a beta version
        /// </summary>
        public bool IsBeta => true;

        /// <summary>
        /// Shortcut to the app user data
        /// </summary>
        public AppUserData Data => RCPServices.Data;

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
        public Pages SelectedPage { get; set; }

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

        #region Public Methods

        /// <summary>
        /// Gets the binary serializer logger to use
        /// </summary>
        /// <returns>The binary serializer logger</returns>
        public IBinarySerializerLogger GetBinarySerializerLogger() => !RCPServices.Data.BinarySerializationFileLogPath.FullPath.IsNullOrWhiteSpace() && RCPServices.Data.BinarySerializationFileLogPath.Parent.DirectoryExists ? new BinarySerializerFileLogger(RCPServices.Data.BinarySerializationFileLogPath) : null;

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
                RL.Logger?.LogDebugSource("A refresh is being requested");

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
            RL.Logger?.LogInformationSource($"The game {game} is being added of type {type}...");

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                RL.Logger?.LogWarningSource($"The game {game} has already been added");

                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, game), Resources.AddGame_DuplicateHeader, MessageType.Error);

                return;
            }

            // Get the manager
            var manager = game.GetManager(type);

            // Add the game
            Data.Games.Add(game, new GameData(type, installDirectory));

            RL.Logger?.LogInformationSource($"The game {game} has been added");

            // Run post-add operations
            await manager.PostGameAddAsync();

            // Add the game to the jump list
            Data.JumpListItemIDCollection.AddRange(manager.GetJumpListItems().Select(x => x.ID));
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
                    Data.JumpListItemIDCollection?.RemoveWhere(x => x == item.ID);

                // Remove game from installed games
                if (Data.InstalledGames.Contains(game))
                    Data.InstalledGames.Remove(game);

                // Remove the game
                Data.Games.Remove(game);

                // Run post game removal
                await manager.PostGameRemovedAsync();
            }
            catch (Exception ex)
            {
                ex.HandleCritical("Removing game");
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
                if (!CommonPaths.UbiIniPath1.FileExists)
                {
                    RL.Logger?.LogInformationSource("The ubi.ini file was not found");
                    return;
                }

                // Check if we have write access
                if (RCPServices.File.CheckFileWriteAccess(CommonPaths.UbiIniPath1))
                {
                    RL.Logger?.LogDebugSource("The ubi.ini file has write access");
                    return;
                }

                await Services.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

                // Attempt to change the permission
                await RunAdminWorkerAsync(AdminWorkerModes.GrantFullControl, CommonPaths.UbiIniPath1);

                RL.Logger?.LogInformationSource($"The ubi.ini file permission was changed");
            }
            catch (Exception ex)
            {
                ex.HandleError("Changing ubi.ini file permissions");
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

                RL.Logger?.LogTraceSource($"The following games were added to the game checker: {games.JoinItems(", ")}");

                // Get additional finder items
                var finderItems = new List<FinderItem>(1);

                // Create DOSBox finder item if it doesn't exist
                if (!File.Exists(Data.DosBoxPath))
                {
                    var names = new string[]
                    {
                        "DosBox",
                        "Dos Box"
                    };

                    void foundAction(FileSystemPath installDir, object parameter)
                    {
                        if (File.Exists(Data.DosBoxPath))
                        {
                            RL.Logger?.LogWarningSource("The DosBox executable was not added from the game finder due to already having been added");
                            return;
                        }

                        RL.Logger?.LogInformationSource("The DosBox executable was found from the game finder");

                        Data.DosBoxPath = installDir + "DOSBox.exe";
                    }

                    finderItems.Add(new FinderItem(names, "DosBox", x => (x + "DOSBox.exe").FileExists ? x : null, foundAction, "DOSBox"));
                }

                // Run the game finder and get the result
                var foundItems = await new GameFinder(games, finderItems).FindGamesAsync();

                // Add the found items
                foreach (var foundItem in foundItems)
                {
                    // Handle the item
                    await foundItem.HandleItemAsync();

                    // If a game, add to list
                    if (foundItem is GameFinderResult game)
                        addedGames.Add(game.Game);
                }

                // Show message if new games were found
                if (foundItems.Count > 0)
                {
                    // Split into found games and items and sort
                    var gameFinderResults = foundItems.OfType<GameFinderResult>().OrderBy(x => x.Game).Select(x => x.DisplayName);
                    var finderResults = foundItems.OfType<FinderResult>().OrderBy(x => x.DisplayName).Select(x => x.DisplayName);

                    await Services.MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {gameFinderResults.Concat(finderResults).JoinItems(Environment.NewLine + "• ")}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);

                    RL.Logger?.LogInformationSource($"The game finder found the following games {foundItems.JoinItems(", ", x => x.ToString())}");

                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Game finder");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameFinder_Error);
            }
            finally
            {
                // Refresh if any games were added
                if (addedGames.Any())
                    await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(addedGames, true, false, false, false));

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
                RL.Logger?.LogInformationSource($"A download is starting...");

                // Make sure there are input sources to download
                if (!inputSources.Any())
                {
                    RL.Logger?.LogInformationSource($"Download failed due to there not being any input sources");

                    await Services.MessageUI.DisplayMessageAsync(Resources.Download_NoFilesFound, MessageType.Error);
                    return false;
                }

                if (Data.HandleDownloadsManually)
                {
                    var result = await RCPServices.UI.DisplayMessageAsync(Resources.Download_ManualInstructions, Resources.Download_ManualHeader, MessageType.Information, true, new DialogMessageActionViewModel[]
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
                                    Task.Run(async () => await RCPServices.File.OpenExplorerLocationAsync(outputDir));
                                }
                            },
                        });

                    if (!result)
                        return false;

                    RL.Logger?.LogInformationSource($"Manual download finished");

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

                        RL.Logger?.LogDebugSource($"The size of the download has been retrieved as {size}");

                        if (!await Services.MessageUI.DisplayMessageAsync(String.Format(isGame ? Resources.DownloadGame_ConfirmSize : Resources.Download_ConfirmSize, size), Resources.Download_ConfirmHeader, MessageType.Question, true))
                            return false;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Getting download size");
                        if (!await Services.MessageUI.DisplayMessageAsync(isGame ? Resources.DownloadGame_Confirm : Resources.Download_Confirm, Resources.Download_ConfirmHeader, MessageType.Question, true))
                            return false;
                    }
                }

                // Create the download dialog
                var dialog = new Downloader(new DownloaderViewModel(inputSources, outputDir, isCompressed));

                // Show the dialog
                dialog.ShowDialog();

                RL.Logger?.LogInformationSource($"The download finished with the result of {dialog.ViewModel.DownloadState}");

                // Return the result
                return dialog.ViewModel.DownloadState == DownloaderViewModel.DownloadStates.Succeeded;
            }
            catch (Exception ex)
            {
                ex.HandleError($"Downloading files");
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
                    RL.Logger?.LogInformationSource("Moving old backups has been canceled by the user");
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
                        RL.Logger?.LogInformationSource("Old backups could not be moved due to not being found");

                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_NoBackupsFound, oldLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);

                        return;
                    }

                    // Make sure the new location doesn't already exist
                    if (newLocation.DirectoryExists)
                    {
                        RL.Logger?.LogInformationSource("Old backups could not be moved due to the new location already existing");

                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_BackupAlreadyExists, newLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                        return;
                    }

                    // Move the directory
                    RCPServices.File.MoveDirectory(oldLocation, newLocation, false, false);

                    RL.Logger?.LogInformationSource("Old backups have been moved");

                    // Refresh backups
                    await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, true, false));

                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.MoveBackups_Success);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Moving backups");
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
            // Lock
            using (await AdminWorkerAsyncLock.LockAsync())
                // Launch the admin worker with the specified launch arguments
                await RCPServices.File.LaunchFileAsync(CommonPaths.AdminWorkerPath, true, $"{mode} {args.Select(x => $"\"{x}\"").JoinItems(" ")}");
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
                ex.HandleError($"Opening URL {url}");
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
                var result = await RCPServices.UpdaterManager.CheckAsync(RCPServices.Data.ForceUpdate && isManualSearch, RCPServices.Data.GetBetaUpdates || IsBeta);

                // Check if there is an error
                if (result.ErrorMessage != null)
                {
                    await Services.MessageUI.DisplayExceptionMessageAsync(result.Exception, result.ErrorMessage, Resources.Update_ErrorHeader);

                    RCPServices.Data.IsUpdateAvailable = false;

                    return;
                }

                // Check if no new updates were found
                if (!result.IsNewUpdateAvailable)
                {
                    if (isManualSearch)
                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, CurrentAppVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                    RCPServices.Data.IsUpdateAvailable = false;

                    return;
                }

                // Indicate that a new update is available
                RCPServices.Data.IsUpdateAvailable = true;

                // Run as new task to mark this operation as finished
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (await Services.MessageUI.DisplayMessageAsync(!result.IsBetaUpdate ? String.Format(Resources.Update_UpdateAvailable, result.DisplayNews) : Resources.Update_BetaUpdateAvailable, Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                        {
                            // Launch the updater and run as admin if set to show under installed programs in under to update the Registry key
                            var succeeded = await RCPServices.UpdaterManager.UpdateAsync(result, false);

                            if (!succeeded)
                                RL.Logger?.LogWarningSource("The updater failed to update the program");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Updating RCP");
                        await Services.MessageUI.DisplayMessageAsync(Resources.Update_Error, Resources.Update_ErrorHeader, MessageType.Error);
                    }
                });
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
                        JsonHelpers.SerializeToFile(RCPServices.Data, CommonPaths.AppUserDataPath);

                        RL.Logger?.LogInformationSource($"The application user data was saved");
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Saving user data");
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
        }

        #endregion
    }
}