using ByteSizeLib;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.RCP.UI;
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
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles common actions and events for this application
    /// </summary>
    public class AppViewModel : BaseRCPAppViewModel<Pages>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppViewModel()
        {
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
                        typeof(R1FixConfigUtility),
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
                        typeof(R2DiscPatchUtility),
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
                        typeof(ROLocalizationConverterUtility),
                        typeof(RODebugCommandsUtility),
                        typeof(ROUpdateUtility),
                    }
                },
                {
                    Games.RaymanLegends,
                    new Type[]
                    {
                        typeof(RLUbiRayUtility),
                        typeof(RLLocalizationConverterUtility),
                        typeof(RLDebugCommandsUtility),
                    }
                },
                {
                    Games.RaymanFiestaRun,
                    new Type[]
                    {
                        typeof(RFRLocalizationConverterUtility),
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
                {
                    Games.PrintStudio,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(PrintStudio_Win32)
                        },
                    }
                },
                {
                    Games.RaymanActivityCenter,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanActivityCenter_Win32)
                        },
                    }
                },
                {
                    Games.RaymanRavingRabbidsActivityCenter,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RaymanRavingRabbidsActivityCenter_Win32)
                        },
                    }
                },
                {
                    Games.GloboxMoment,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(GloboxMoment_Win32)
                        },
                    }
                },
                {
                    Games.TheDarkMagiciansReignofTerror,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(TheDarkMagiciansReignofTerror_Win32)
                        },
                    }
                },
                {
                    Games.RabbidsCoding,
                    new Dictionary<GameType, Type>()
                    {
                        {
                            GameType.Win32,
                            typeof(RabbidsCoding_Win32)
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
                {
                    Games.PrintStudio,
                    typeof(PrintStudio_Info)
                },
                {
                    Games.RaymanActivityCenter,
                    typeof(RaymanActivityCenter_Info)
                },
                {
                    Games.RaymanRavingRabbidsActivityCenter,
                    typeof(RaymanRavingRabbidsActivityCenter_Info)
                },
                {
                    Games.GloboxMoment,
                    typeof(GloboxMoment_Info)
                },
                {
                    Games.TheDarkMagiciansReignofTerror,
                    typeof(TheDarkMagiciansReignofTerror_Info)
                },
                {
                    Games.RabbidsCoding,
                    typeof(RabbidsCoding_Info)
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
        /// The name of the backup directory for this application
        /// </summary>
        public const string BackupFamily = "Rayman Game Backups";

        #endregion

        #region Public Static Properties

        // IDEA: Move to resource dictionary as static resource
        /// <summary>
        /// The path to the resource file
        /// </summary>
        public static string ResourcePath => "RayCarrot.RCP.Metro.Localization.Resources";

        #endregion

        #region Private Properties

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
        /// Shortcut to the app user data
        /// </summary>
        public AppUserData Data => RCFRCP.Data;

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
        /// A flag indicating if an update check is in progress
        /// </summary>
        public bool CheckingForUpdates { get; set; }

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentVersion => new Version(7, 2, 0, 0);

        /// <summary>
        /// Indicates if the current version is a beta version
        /// </summary>
        public bool IsBeta => false;

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
                if (!RCFRCP.Path.UbiIniPath1.FileExists)
                {
                    RCFCore.Logger?.LogInformationSource("The ubi.ini file was not found");
                    return;
                }

                // Check if we have write access
                if (RCFRCPA.File.CheckFileWriteAccess(RCFRCP.Path.UbiIniPath1))
                {
                    RCFCore.Logger?.LogDebugSource("The ubi.ini file has write access");
                    return;
                }

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

                // Attempt to change the permission
                await RunAdminWorkerAsync(AdminWorkerModes.GrantFullControl, RCFRCP.Path.UbiIniPath1);

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
                            RCFCore.Logger?.LogWarningSource("The DosBox executable was not added from the game finder due to already having been added");
                            return;
                        }

                        RCFCore.Logger?.LogInformationSource("The DosBox executable was found from the game finder");

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

                    await RCFUI.MessageUI.DisplayMessageAsync($"{Resources.GameFinder_GamesFound}{Environment.NewLine}{Environment.NewLine}• {gameFinderResults.Concat(finderResults).JoinItems(Environment.NewLine + "• ")}", Resources.GameFinder_GamesFoundHeader, MessageType.Success);

                    RCFCore.Logger?.LogInformationSource($"The game finder found the following games {foundItems.JoinItems(", ", x => x.ToString())}");

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
        /// <param name="isGame">Indicates if the download is for a game. If false it is assumed to be a generic patch.</param>
        /// <returns>True if the download succeeded, otherwise false</returns>
        public async Task<bool> DownloadAsync(IList<Uri> inputSources, bool isCompressed, FileSystemPath outputDir, bool isGame = false)
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"A download is starting...");

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

                    if (!await RCFUI.MessageUI.DisplayMessageAsync(String.Format(isGame ? Resources.DownloadGame_ConfirmSize : Resources.Download_ConfirmSize, size), Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting download size");
                    if (!await RCFUI.MessageUI.DisplayMessageAsync(isGame ? Resources.DownloadGame_Confirm : Resources.Download_Confirm, Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }

                // Create the download dialog
                var dialog = new Downloader(new DownloaderViewModel(inputSources, outputDir, isCompressed));

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
                CheckingForUpdates = true;

                // Check for updates
                var result = await RCFRCPA.UpdaterManager.CheckAsync(RCFRCP.Data.ForceUpdate && isManualSearch, RCFRCP.Data.GetBetaUpdates || RCFRCP.App.IsBeta);

                // Check if there is an error
                if (result.ErrorMessage != null)
                {
                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(result.Exception, result.ErrorMessage, Resources.Update_ErrorHeader);

                    Data.IsUpdateAvailable = false;

                    return;
                }

                // Check if no new updates were found
                if (!result.IsNewUpdateAvailable)
                {
                    if (isManualSearch)
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, CurrentVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                    Data.IsUpdateAvailable = false;

                    return;
                }

                // Indicate that a new update is available
                Data.IsUpdateAvailable = true;

                // Run as new task to mark this operation as finished
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (await RCFUI.MessageUI.DisplayMessageAsync(!result.IsBetaUpdate ? String.Format(Resources.Update_UpdateAvailable, result.DisplayNews) : Resources.Update_BetaUpdateAvailable, Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                        {
                            try
                            {
                                Directory.CreateDirectory(RCFRCP.Path.UpdaterFile.Parent);
                                File.WriteAllBytes(RCFRCP.Path.UpdaterFile, Files.Rayman_Control_Panel_Updater);
                                RCFCore.Logger?.LogInformationSource($"The updater was created");
                            }
                            catch (Exception ex)
                            {
                                ex.HandleError("Writing updater to temp path", RCFRCP.Path.UpdaterFile);
                                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Update_UpdaterError, "raycarrot.ylemnova.com"), Resources.Update_UpdaterErrorHeader);
                                return;
                            }

                            // Launch the updater and run as admin is set to show under installed programs in under to update the Registry key
                            if (await RCFRCPA.File.LaunchFileAsync(RCFRCP.Path.UpdaterFile, Data.ShowUnderInstalledPrograms, $"\"{Assembly.GetExecutingAssembly().Location}\" {RCFRCP.Data.DarkMode} {RCFRCP.Data.UserLevel} {result.IsBetaUpdate} \"{RCFCore.Data.CurrentCulture}\"") == null)
                            {
                                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_RunningUpdaterError, "raycarrot.ylemnova.com"), Resources.Update_RunningUpdaterErrorHeader, MessageType.Error);

                                return;
                            }

                            // Shut down the app
                            await App.Current.ShutdownRCFAppAsync(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Updating RCP");
                        await RCFUI.MessageUI.DisplayMessageAsync(Resources.Update_Error, Resources.Update_ErrorHeader, MessageType.Error);
                    }
                });
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

                    RCFRCPA.File.MoveDirectory(oldLocation, newLocation, false, false);

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
                await RCFRCPA.File.LaunchFileAsync(RCFRCP.Path.AdminWorkerPath, true, $"{mode} {args.Select(x => $"\"{x}\"").JoinItems(" ")}");
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
                if (overwrite || !RCFRCP.Path.UninstallFilePath.FileExists)
                {
                    Directory.CreateDirectory(RCFRCP.Path.UninstallFilePath.Parent);
                    File.WriteAllBytes(RCFRCP.Path.UninstallFilePath, Files.Uninstaller);
                }

                // Deploy the admin worker
                if (overwrite || !RCFRCP.Path.AdminWorkerPath.FileExists)
                {
                    Directory.CreateDirectory(RCFRCP.Path.AdminWorkerPath.Parent);
                    File.WriteAllBytes(RCFRCP.Path.AdminWorkerPath, Files.AdminWorker);
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
            if (await RCFUI.MessageUI.DisplayMessageAsync(Resources.App_RequiresAdminQuestion, Resources.App_RestartAsAdmin, MessageType.Warning, true))
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