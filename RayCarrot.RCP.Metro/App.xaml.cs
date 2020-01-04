using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using RayCarrot.WPF.Metro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseRCPApp<MainWindow, AppUserData, RCPMetroAPIControllerManager>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public App() : base(true, "Files/Splash Screen.png")
        {
            SplashScreenFadeout = TimeSpan.FromMilliseconds(200);
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Sets up the framework with loggers and other services
        /// </summary>
        /// <param name="config">The configuration values to pass on to the framework, if any</param>
        /// <param name="logLevel">The level to log</param>
        /// <param name="args">The launch arguments</param>
        protected override void SetupFramework(IDictionary<string, object> config, LogLevel logLevel, string[] args)
        {
            // Add custom configuration
            config.Add(RCFIO.AutoCorrectPathCasingKey,
                // IDEA: Set to true?
                false);

            // Set file log level
            FileLogger.FileLoggerLogLevel = logLevel;

            new FrameworkConstruction().
                // Add loggers
                AddLoggers(DefaultLoggers.Console | DefaultLoggers.Debug | DefaultLoggers.Session, logLevel, builder => builder.AddProvider(new BaseLogProvider<FileLogger>())).
                // Add user data manager
                AddUserDataManager(() => new JsonBaseSerializer()
                {
                    Formatting = Formatting.Indented
                }).
                // Add the app view model
                AddAppViewModel<AppViewModel>().
                // Add App UI manager
                AddTransient<AppUIManager>().
                // Add application paths
                AddApplicationPaths<RCPMetroApplicationPaths>().
                // Add localization manager
                AddLocalizationManager<RCPMetroLocalizationManager>().
                // Add backup manager
                AddTransient<BackupManager>().
                // Add API controller manager
                AddAPIControllerManager<RCPMetroAPIControllerManager>().
                // Build the framework
                Build(config);
        }

        /// <summary>
        /// Handles a change in the app user data
        /// </summary>
        /// <param name="propertyName">The name of the property which changed</param>
        /// <returns>The task</returns>
        protected override async Task AppUserDataChanged(string propertyName)
        {
            await base.AppUserDataChanged(propertyName);

            switch (propertyName)
            {
                case nameof(AppUserData.BackupLocation):

                    await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, true, false));

                    if (!PreviousBackupLocation.DirectoryExists)
                    {
                        RCFCore.Logger?.LogInformationSource("The backup location has been changed, but the previous directory does not exist");
                        return;
                    }

                    RCFCore.Logger?.LogInformationSource("The backup location has been changed and old backups are being moved...");

                    await RCFRCP.App.MoveBackupsAsync(PreviousBackupLocation, AppData.BackupLocation);

                    PreviousBackupLocation = AppData.BackupLocation;

                    break;

                case nameof(AppUserData.LinkItemStyle):
                    static string GetStyleSource(LinkItemStyles linkItemStye) => $"{RCFRCP.App.WPFApplicationBasePath}/Styles/LinkItemStyles - {linkItemStye}.xaml";

                    // Get previous source
                    var oldSource = GetStyleSource(PreviousLinkItemStyle);

                    // Remove old source
                    foreach (ResourceDictionary resourceDictionary in Resources.MergedDictionaries)
                    {
                        if (!String.Equals(resourceDictionary.Source?.ToString(), oldSource,
                            StringComparison.OrdinalIgnoreCase))
                            continue;

                        Resources.MergedDictionaries.Remove(resourceDictionary);
                        break;
                    }

                    // Add new source
                    Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri(GetStyleSource(AppData.LinkItemStyle))
                    });

                    PreviousLinkItemStyle = AppData.LinkItemStyle;

                    break;

                case nameof(AppUserData.RRR2LaunchMode):
                    await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanRavingRabbids2, false, false, false, true));
                    break;

                case nameof(AppUserData.DosBoxPath):
                case nameof(AppUserData.DosBoxConfig):
                    await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, false, true));
                    break;
            }
        }

        /// <summary>
        /// Runs the basic startup, such as handling launch arguments
        /// </summary>
        /// <returns>The task</returns>
        protected override async Task BasicStartupAsync()
        {
            // Track changes to the user data
            PreviousLinkItemStyle = AppData.LinkItemStyle;
            PreviousBackupLocation = AppData.BackupLocation;

            // Subscribe to when to refresh the jump list
            RCFRCP.App.RefreshRequired += (s, e) =>
            {
                if (e.GameCollectionModified || e.GameInfoModified || e.JumpListModified)
                    RefreshJumpList();

                return Task.CompletedTask;
            };
            RCFCore.Data.CultureChanged += (s, e) => RefreshJumpList();

            // Subscribe to when the app has finished setting up
            StartupComplete += App_StartupComplete_GameFinder_Async;
            StartupComplete += App_StartupComplete_Miscellaneous_Async;

            // Run base
            await base.BasicStartupAsync();

            await RCFRCP.App.DeployFilesAsync(false);

            // Show first launch info
            if (AppData.IsFirstLaunch)
            {
                // Close the splash screen
                CloseSplashScreen();

                new FirstLaunchInfoDialog().ShowDialog();
                AppData.IsFirstLaunch = false;
            }

            LogStartupTime("Validating games");

            await ValidateGamesAsync();

            LogStartupTime("Finished validating games");
        }

        /// <summary>
        /// Runs the post-update code
        /// </summary>
        protected override async Task PostUpdateAsync()
        {
            // Run base
            await base.PostUpdateAsync();

            if (AppData.LastVersion < new Version(4, 0, 0, 6))
                AppData.EnableAnimations = true;

            if (AppData.LastVersion < new Version(4, 1, 1, 0))
                AppData.ShowIncompleteTranslations = false;

            if (RCFRCP.Data.LastVersion < new Version(4, 5, 0, 0))
            {
                AppData.LinkItemStyle = LinkItemStyles.List;
                AppData.ApplicationPath = Assembly.GetEntryAssembly()?.Location;
                AppData.ForceUpdate = false;
                AppData.GetBetaUpdates = false;
            }

            if (AppData.LastVersion < new Version(4, 6, 0, 0))
                AppData.LinkListHorizontalAlignment = HorizontalAlignment.Left;

            if (AppData.LastVersion < new Version(5, 0, 0, 0))
            {
                AppData.CompressBackups = true;
                AppData.FiestaRunVersion = FiestaRunEdition.Default;

                // Due to the fiesta run version system being changed the game has to be removed and then re-added
                AppData.Games.Remove(Games.RaymanFiestaRun);

                // If a Fiesta Run backup exists the name needs to change to the new standard
                var fiestaBackupDir = RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily + "Rayman Fiesta Run";

                if (fiestaBackupDir.DirectoryExists)
                {
                    try
                    {
                        // Read the app data file
                        JObject appData = new StringReader(File.ReadAllText(RCFRCP.Data.FilePath)).RunAndDispose(x =>
                            new JsonTextReader(x).RunAndDispose(y => JsonSerializer.Create().Deserialize(y))).CastTo<JObject>();

                        // Get the previous Fiesta Run version
                        var isWin10 = appData["IsFiestaRunWin10Edition"].Value<bool>();

                        // Set the current edition
                        AppData.FiestaRunVersion = isWin10 ? FiestaRunEdition.Win10 : FiestaRunEdition.Default;

                        RCFRCPC.File.MoveDirectory(fiestaBackupDir, RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily + Games.RaymanFiestaRun.GetGameInfo().BackupName, true, true);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Moving Fiesta Run backups to 5.0.0 standard");

                        await RCFUI.MessageUI.DisplayMessageAsync(Metro.Resources.PostUpdate_MigrateFiestaRunBackup5Error, Metro.Resources.PostUpdate_MigrateBackupErrorHeader, MessageType.Error);
                    }
                }

                // Remove old temp dir
                try
                {
                    RCFRCPC.File.DeleteDirectory(Path.Combine(Path.GetTempPath(), "RCP_Metro"));
                }
                catch (Exception ex)
                {
                    ex.HandleError("Cleaning pre-5.0.0 temp");
                }

                AppData.DisableDowngradeWarning = false;
            }

            if (AppData.LastVersion < new Version(6, 0, 0, 0))
            {
                AppData.EducationalDosBoxGames = null;
                AppData.RRR2LaunchMode = RRR2LaunchMode.AllGames;
                AppData.RabbidsGoHomeLaunchData = null;
            }

            if (AppData.LastVersion < new Version(6, 0, 0, 2))
            {
                // By default, add all games to the jump list collection
                AppData.JumpListItemIDCollection = RCFRCP.App.GetGames.
                    Where(x => x.IsAdded()).
                    Select(x => x.GetManager().GetJumpListItems().Select(y => y.ID)).
                    SelectMany(x => x).
                    ToList();
            }

            if (AppData.LastVersion < new Version(7, 0, 0, 0))
            {
                AppData.IsUpdateAvailable = false;

                if (AppData.UserLevel == UserLevel.Normal)
                    AppData.UserLevel = UserLevel.Advanced;
            }

            if (AppData.LastVersion < new Version(7, 1, 0, 0))
                AppData.InstalledGames = new HashSet<Games>();

            if (AppData.LastVersion < new Version(7, 1, 1, 0))
                AppData.CategorizeGames = true;

            if (AppData.LastVersion < new Version(7, 2, 0, 0))
                AppData.ShownRabbidsActivityCenterLaunchMessage = false;

            if (AppData.LastVersion < new Version(8, 1, 0, 0))
            {
                // Since support has been removed for showing the program under installed programs we now have to remove the key
                var keyPath = RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, RCFRCPC.API.AppCodeName);

                if (RCFWinReg.RegistryManager.KeyExists(keyPath))
                {
                    if (RCFRCP.App.IsRunningAsAdmin)
                    {
                        try
                        {
                            using var parentKey = RCFWinReg.RegistryManager.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms, RegistryView.Default, true);

                            parentKey.DeleteSubKey(RCFRCPC.API.AppCodeName);

                            RCFCore.Logger?.LogInformationSource("The program Registry key has been deleted");
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Removing uninstall Registry key");

                            await RCFUI.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                        }
                    }
                    else
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                    }
                }
            }

            // Re-deploy files
            await RCFRCP.App.DeployFilesAsync(true);

            // Refresh the jump list
            RefreshJumpList();

            // Close the splash screen
            CloseSplashScreen();

            // Show app news
            new AppNewsDialog().ShowDialog();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the added games
        /// </summary>
        /// <returns></returns>
        private static async Task ValidateGamesAsync()
        {
            // Keep track of removed games
            var removed = new List<Games>();

            // Make sure every game is valid
            foreach (var game in RCFRCP.App.GetGames)
            {
                // Check if it has been added
                if (!game.IsAdded())
                    continue;

                // Check if it's valid
                if (await game.GetManager().IsValidAsync(game.GetInstallDir()))
                    continue;

                // Show message
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.GameNotFound, game.GetGameInfo().DisplayName), Metro.Resources.GameNotFoundHeader, MessageType.Error);

                // Remove the game from app data
                await RCFRCP.App.RemoveGameAsync(game, true);

                // Add to removed games
                removed.Add(game);

                RCFCore.Logger?.LogInformationSource($"The game {game} has been removed due to not being valid");
            }

            // Refresh if any games were removed
            if (removed.Any())
                await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(removed, true, false, false, false));
        }

        #endregion

        #region Event Handlers

        private async Task App_StartupComplete_Miscellaneous_Async(object sender, EventArgs eventArgs)
        {
            if (Dispatcher == null)
                throw new Exception("Dispatcher is null");

            // Run on UI thread
            Dispatcher.Invoke(SecretCodeManager.Setup);

            // Enable primary ubi.ini file write access
            await RCFRCP.App.EnableUbiIniWriteAccessAsync();
        }

        private static async Task App_StartupComplete_GameFinder_Async(object sender, EventArgs eventArgs)
        {
            // Check for installed games
            if (RCFRCP.Data.AutoLocateGames)
                await RCFRCP.App.RunGameFinderAsync();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the application jump list
        /// </summary>
        public void RefreshJumpList()
        {
            Dispatcher?.Invoke(() =>
            {
                try
                {
                    if (RCFRCP.Data.JumpListItemIDCollection == null)
                    {
                        RCFCore.Logger?.LogWarningSource("The jump could not refresh due to collection not existing");

                        return;
                    }

                    // Create a jump list
                    new JumpList(RCFRCP.App.GetGames.
                            // Add only games which have been added
                            Where(x => x.IsAdded()).
                            // Get the items for each game
                            Select(x => x.GetManager().GetJumpListItems()).
                            // Select into single collection
                            SelectMany(x => x).
                            // Keep only the included items
                            Where(x => x.IsIncluded).
                            // Keep custom order
                            OrderBy(x => RCFRCP.Data.JumpListItemIDCollection.IndexOf(x.ID)).
                            // Create the jump tasks
                            Select(x => new JumpTask
                            {
                                Title = x.Name,
                                Description = String.Format(Metro.Resources.JumpListItemDescription, x.Name),
                                ApplicationPath = x.LaunchPath,
                                WorkingDirectory = x.WorkingDirectory,
                                Arguments = x.LaunchArguments,
                                IconResourcePath = x.IconSource
                            }), false, false).
                        // Apply the new jump list
                        Apply();

                    RCFCore.Logger?.LogInformationSource("The jump list has been refreshed");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Creating jump list");
                }
            });
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The saved previous backup location
        /// </summary>
        private FileSystemPath PreviousBackupLocation { get; set; }

        /// <summary>
        /// The saved previous link item style
        /// </summary>
        private LinkItemStyles PreviousLinkItemStyle { get; set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion
    }
}