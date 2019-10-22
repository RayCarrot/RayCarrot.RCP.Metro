using ByteSizeLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Shell;
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
            IsStartupRunning = true;

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
                        typeof(RLDebugCommandsUtility),
                    }
                },
            };
            GameManagers = new Dictionary<Games, Dictionary<GameType, Type>>()
            {
                {
                    Games.Rayman1,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.DosBox,
                            typeof(Rayman1_DOSBox)
                        },
                    }
                },
                {
                    Games.RaymanDesigner,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.DosBox,
                            typeof(RaymanDesigner_DOSBox)
                        },
                    }
                },
                {
                    Games.RaymanByHisFans,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.DosBox,
                            typeof(RaymanByHisFans_DOSBox)
                        },
                    }
                },
                {
                    Games.Rayman60Levels,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.DosBox,
                            typeof(Rayman60Levels_DOSBox)
                        },
                    }
                },
                {
                    Games.Rayman2,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(Rayman2_Win32)
                        },
                        {
                            GameType.Steam,
                            typeof(Rayman2_Steam)
                        },
                    }
                },
                {
                    Games.RaymanM,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanM_Win32)
                        },
                    }
                },
                {
                    Games.RaymanArena,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanArena_Win32)
                        },
                    }
                },
                {
                    Games.Rayman3,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(Rayman3_Win32)
                        },
                    }
                },
                {
                    Games.RaymanRavingRabbids,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanRavingRabbids_Win32)
                        },
                        {
                            GameType.Steam,
                            typeof(RaymanRavingRabbids_Steam)
                        },
                    }
                },
                {
                    Games.RaymanRavingRabbids2,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanRavingRabbids2_Win32)
                        },
                    }
                },
                {
                    Games.RabbidsGoHome,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RabbidsGoHome_Win32)
                        },
                    }
                },
                {
                    Games.RaymanOrigins,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanOrigins_Win32)
                        },
                        {
                            GameType.Steam,
                            typeof(RaymanOrigins_Steam)
                        },
                    }
                },
                {
                    Games.RaymanLegends,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanLegends_Win32)
                        },
                        {
                            GameType.Steam,
                            typeof(RaymanLegends_Steam)
                        },
                    }
                },
                {
                    Games.RaymanJungleRun,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.WinStore,
                            typeof(RaymanJungleRun_WinStore)
                        },
                    }
                },
                {
                    Games.RaymanFiestaRun,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.WinStore,
                            typeof(RaymanFiestaRun_WinStore)
                        },
                    }
                },
                {
                    Games.RabbidsBigBang,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.WinStore,
                            typeof(RabbidsBigBang_WinStore)
                        },
                    }
                },
                {
                    Games.EducationalDos,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.EducationalDosBox,
                            typeof(EducationalDos_EducationalDOSBox)
                        },
                    }
                },
            };
            GameInfos = new Dictionary<Games, Type>()
            {
                {
                    Games.Rayman1,
                    typeof(Rayman1_Info)
                },
                {
                    Games.RaymanDesigner,
                    typeof(RaymanDesigner_Info)
                },
                {
                    Games.RaymanByHisFans,
                    typeof(RaymanByHisFans_Info)
                },
                {
                    Games.Rayman60Levels,
                    typeof(Rayman60Levels_Info)
                },
                {
                    Games.Rayman2,
                    typeof(Rayman2_Info)
                },
                {
                    Games.RaymanM,
                    typeof(RaymanM_Info)
                },
                {
                    Games.RaymanArena,
                    typeof(RaymanArena_Info)
                },
                {
                    Games.Rayman3,
                    typeof(Rayman3_Info)
                },
                {
                    Games.RaymanRavingRabbids,
                    typeof(RaymanRavingRabbids_Info)
                },
                {
                    Games.RaymanRavingRabbids2,
                    typeof(RaymanRavingRabbids2_Info)
                },
                {
                    Games.RabbidsGoHome,
                    typeof(RabbidsGoHome_Info)
                },
                {
                    Games.RaymanOrigins,
                    typeof(RaymanOrigins_Info)
                },
                {
                    Games.RaymanLegends,
                    typeof(RaymanLegends_Info)
                },
                {
                    Games.RaymanJungleRun,
                    typeof(RaymanJungleRun_Info)
                },
                {
                    Games.RaymanFiestaRun,
                    typeof(RaymanFiestaRun_Info)
                },
                {
                    Games.RabbidsBigBang,
                    typeof(RabbidsBigBang_Info)
                },
                {
                    Games.EducationalDos,
                    typeof(EducationalDos_Info)
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
        /// Indicates if the application startup operation is running
        /// </summary>
        public bool IsStartupRunning { get; set; }

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
        public Version CurrentVersion => new Version(7, 0, 0, 0);

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
        /// <param name="installDirectory">The game install directory</param>
        /// <returns>The task</returns>
        public async Task AddNewGameAsync(Games game, GameType type, FileSystemPath installDirectory)
        {
            RCFCore.Logger?.LogInformationSource($"The game {game} is being added of type {type}...");

            // Make sure the game hasn't already been added
            if (game.IsAdded())
            {
                RCFCore.Logger?.LogWarningSource($"The game {game} has already been added");

                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, game), Resources.AddGame_DuplicateHeader, MessageType.Error);

                return;
            }

            // Get the manager
            var manager = game.GetManager(type);

            // Add the game
            Data.Games.Add(game, new GameData(type, installDirectory));

            RCFCore.Logger?.LogInformationSource($"The game {game} has been added");

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
                    var utilities = await game.GetGameInfo().GetAppliedUtilitiesAsync();

                    // Warn about utilities
                    if (utilities.Any() && !await RCFUI.MessageUI.DisplayMessageAsync(
                            $"{Resources.RemoveGame_UtilityWarning}{Environment.NewLine}{Environment.NewLine}{utilities.JoinItems(Environment.NewLine)}", Resources.RemoveGame_UtilityWarningHeader, MessageType.Warning, true))
                        return;
                }

                // Remove the game from the jump list
                foreach (var item in manager.GetJumpListItems())
                    Data.JumpListItemIDCollection?.RemoveWhere(x => x == item.ID);

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
        /// Saves all user data for the application
        /// </summary>
        public async Task SaveUserDataAsync()
        {
            // Lock the saving of user data
            using (await SaveUserDataAsyncLock.LockAsync())
            {
                // Run it as a new task
                await Task.Run(async () =>
                {
                    // Save all user data
                    try
                    {
                        // Save all user data
                        await RCFData.UserDataCollection.SaveAllAsync();

                        RCFCore.Logger?.LogInformationSource($"The application user data was saved");
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Saving user data");
                    }
                });
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
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.UbiIniWriteAccess_Error);
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

                RCFCore.Logger?.LogTraceSource($"The following games were added to the game checker: {games.JoinItems(", ")}");

                // Create DOSBox finder item if it doesn't exist
                var finderItems = !File.Exists(Data.DosBoxPath)
                    ? new FinderItem[]
                    {
                            new FinderItem(new string[]
                            {
                                "DosBox",
                                "Dos Box"
                            }, "DosBox", x => (x + "DOSBox.exe").FileExists ? x : null, x =>
                            {
                                if (File.Exists(Data.DosBoxPath))
                                {
                                    RCFCore.Logger?.LogWarningSource(
                                        $"The DosBox executable was not added from the game finder due to already having been added");
                                    return;
                                }

                                RCFCore.Logger?.LogInformationSource(
                                    $"The DosBox executable was found from the game finder");

                                Data.DosBoxPath = x + "DOSBox.exe";
                            })
                    }
                    : new FinderItem[0];

                // Run the game finder and get the result
                var foundGames = RCFRCP.GameFinder.FindGames(games, finderItems);

                // Add the found games
                foreach (var gameResult in foundGames)
                {
                    // Add the game
                    await AddNewGameAsync(gameResult.Game, gameResult.GameType, gameResult.InstallLocation);

                    // Add to list
                    addedGames.Add(gameResult.Game);
                }

                // Show message if new games were found
                if (foundGames.Count > 0)
                {
                    await RCFUI.MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {foundGames.OrderBy(x => x.Game).JoinItems(Environment.NewLine + "• ", x => x.Game.GetGameInfo().DisplayName)}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);

                    RCFCore.Logger?.LogInformationSource($"The game finder found the following games {foundGames.JoinItems(", ", x => x.Game.ToString())}");

                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Game finder");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameFinder_Error);
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

                        using var webResponse = webRequest.GetResponse();
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
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Download_Error);
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
                    using var wc = new WebClient();
                    var result = await wc.DownloadStringTaskAsync(CommonUrls.UpdateManifestUrl);
                    manifest = JObject.Parse(result);
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
                        await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Update_UpdaterError, "raycarrot.ylemnova.com"), Resources.Update_UpdaterErrorHeader);
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

                    RCFRCP.File.MoveDirectory(oldLocation, newLocation, false, false);

                    RCFCore.Logger?.LogInformationSource("Old backups have been moved");

                    await OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, true, false));

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.MoveBackups_Success);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Moving backups");
                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.MoveBackups_Error, Resources.MoveBackups_ErrorHeader);
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

                await game.GetManager(typeResult.SelectedType).LocateAddGameAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Locating game");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader);
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
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.DeployFilesError);
            }
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

        #region Events

        /// <summary>
        /// Occurs when a refresh is required for the app
        /// </summary>
        public event AsyncEventHandler<RefreshRequiredEventArgs> RefreshRequired;

        #endregion
    }
}