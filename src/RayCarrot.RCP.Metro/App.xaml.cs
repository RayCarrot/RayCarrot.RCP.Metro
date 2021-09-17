using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.Windows.Registry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseApp
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public App() : base(true, "Files/Splash Screen.png")
        {
            // Set properties
            DataChangedHandlerAsyncLock = new AsyncLock();
            SplashScreenFadeout = TimeSpan.FromMilliseconds(200);

            // Subscribe to events
            StartupEventsCompleted += App_StartupEventsCompleted;
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Gets the services to use for the application
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>The services to use</returns>
        protected override IServiceCollection GetServices(string[] args)
        {
            InitializeLogging(args);

            // Add services
            return new ServiceCollection().
                // Add user data
                AddSingleton(new AppUserData()).
                // Add message UI manager
                AddMessageUIManager<RCPMessageUIManager>().
                // Add browse UI manager
                AddBrowseUIManager<RCPBrowseUIManager>().
                // Add file manager
                AddFileManager<RCPFileManager>().
                // Add dialog base manager
                AddDialogBaseManager<RCPWindowDialogBaseManager>().
                // Add update manager
                AddUpdateManager<RCPUpdaterManager>().
                // Add the app view model
                AddSingleton(new AppViewModel(x => LogStartupTime(x))).
                // Add App UI manager
                AddTransient<AppUIManager>().
                // Add backup manager
                AddTransient<GameBackups_Manager>();
        }

        /// <summary>
        /// An optional custom setup to override
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>The task</returns>
        protected override async Task OnSetupAsync(string[] args)
        {
            LogStartupTime("Setup: RCP setup is starting");

            // Load the user data
            try
            {
                // Read the data from the file if it exists
                if (!Services.InstanceData.Arguments.Contains("-reset") && AppFilePaths.AppUserDataPath.FileExists)
                {
                    // Always reset the data first so any missing properties use the correct defaults
                    Services.Data.Reset();

                    Services.Data.App_LastVersion = null; // Need to set to null before calling JsonConvert.PopulateObject or else it's ignored

                    // Populate the data from the file
                    JsonConvert.PopulateObject(File.ReadAllText(AppFilePaths.AppUserDataPath), Services.Data);

                    Logger.Info("The app user data has been loaded");

                    // Verify the data
                    Services.Data.Verify();
                }
                else
                {
                    // Reset the user data
                    Services.Data.Reset();

                    Logger.Info("The app user data has been reset");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Loading app user data");

                // NOTE: This is not localized due to the current culture not having been set at this point
                MessageBox.Show("An error occurred reading saved app data. Some settings have been reset to their default values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Reset the user data
                Services.Data.Reset();
            }

            Data = Services.Data;

            LogStartupTime("Setup: Setting theme");

            // Set the theme
            this.SetTheme(Data.Theme_DarkMode, Data.Theme_SyncTheme);

            LogStartupTime("Setup: Setting culture");

            // Apply the current culture if defaulted
            if (Data.App_CurrentCulture == LocalizationManager.DefaultCulture.Name)
                LocalizationManager.SetCulture(LocalizationManager.DefaultCulture.Name);

            LogStartupTime("Setup: Setup WPF trace listener");

            // Listen to data binding logs
            WPFTraceListener.Setup();

            StartupComplete += BaseApp_StartupComplete_Miscellaneous_Async;
            StartupComplete += App_StartupComplete_Updater_Async;
            Data.PropertyChanged += Data_PropertyChangedAsync;

            // Run basic startup
            await BasicStartupAsync();

            Logger.Info("Current version is {0}", Services.App.CurrentAppVersion);

            // Check if it's a new version
            if (Data.App_LastVersion < Services.App.CurrentAppVersion)
            {
                // Run post-update code
                await PostUpdateAsync();

                LogStartupTime("Setup: Post update has run");

                // Update the last version
                Data.App_LastVersion = Services.App.CurrentAppVersion;
            }
            // Check if it's a lower version than previously recorded
            else if (Data.App_LastVersion > Services.App.CurrentAppVersion)
            {
                Logger.Warn("A newer version ({0}) has been recorded in the application data", Data.App_LastVersion);

                if (!Data.Update_DisableDowngradeWarning)
                    await Services.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.DowngradeWarning, Services.App.CurrentAppVersion,
                        Data.App_LastVersion), Metro.Resources.DowngradeWarningHeader, MessageType.Warning);
            }
        }

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected override Window GetMainWindow()
        {
            // Create the window
            var window = new MainWindow();

            // Load previous state
            Services.Data?.UI_WindowState?.ApplyToWindow(window);

            return window;
        }

        /// <summary>
        /// An optional method to override which runs when closing
        /// </summary>
        /// <param name="mainWindow">The main Window of the application</param>
        /// <returns>The task</returns>
        protected override async Task OnCloseAsync(Window mainWindow)
        {
            // Make sure the user data has been loaded
            if (Services.Data != null)
            {
                // Save window state
                Services.Data.UI_WindowState = UserData_WindowSessionState.GetWindowState(mainWindow);

                Logger.Info("The application is exiting...");

                // Save all user data
                await Services.App.SaveUserDataAsync();
            }

            // Close the logger
            LogManager.Shutdown();
        }

        /// <summary>
        /// Override to run when another instance of the program is found running
        /// </summary>
        /// <param name="args">The launch arguments</param>
        protected override void OnOtherInstanceFound(string[] args)
        {
            MessageBox.Show($"An instance of the Rayman Control Panel is already running", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Optional initial setup to run. Can be used to check if the environment is valid
        /// for the application to run or for the user to accept the license.
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>True if the setup finished successfully or false if the application has to shut down</returns>
        protected override Task<bool> InitialSetupAsync(string[] args)
        {
            LogStartupTime("InitialSetup: Checking Windows version");

            // Make sure we are on Windows Vista or higher for the Windows API Code Pack and Deployment Image Servicing and Management
            if (AppViewModel.WindowsVersion < WindowsVersion.WinVista && AppViewModel.WindowsVersion != WindowsVersion.Unknown)
            {
                MessageBox.Show("Windows Vista or higher is required to run this application", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult(false);
            }

            LogStartupTime("InitialSetup: Checking license");

            // Make sure the license has been accepted
            if (!ShowLicense())
                return Task.FromResult(false);

            LogStartupTime("InitialSetup: Setting default directory");

            // Hard code the current directory
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Directory.GetCurrentDirectory());

            return Task.FromResult(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the application license message and returns a value indicating if it was accepted
        /// </summary>
        /// <returns>True if it was accepted, false if not</returns>
        private bool ShowLicense()
        {
            try
            {
                // Get the license value, if one exists
                int regValue = Registry.GetValue(AppFilePaths.RegistryBaseKey, AppFilePaths.RegistryLicenseValue, 0)?.CastTo<int>() ?? 0;

                // Check if it has been accepted
                if (regValue == 1)
                    return true;

                // Create license popup dialog
                var licenseDialog = new LicenseDialog();

                // Close the splash screen
                CloseSplashScreen();

                // Show the dialog
                licenseDialog.ShowDialog();

                // Set Registry value if accepted
                if (licenseDialog.Accepted)
                    Registry.SetValue(AppFilePaths.RegistryBaseKey, AppFilePaths.RegistryLicenseValue, 1);

                // Return if it was accepted
                return licenseDialog.Accepted;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The license verification failed with the message of: {ex.Message}", "License error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Runs the basic startup, such as handling launch arguments
        /// </summary>
        /// <returns>The task</returns>
        private async Task BasicStartupAsync()
        {
            LogStartupTime("BasicStartup: Start basic startup");

            // Track changes to the user data
            PreviousLinkItemStyle = Data.UI_LinkItemStyle;
            PreviousBackupLocation = Data.Backup_BackupLocation;

            // Subscribe to when to refresh the jump list
            Services.App.RefreshRequired += (_, e) =>
            {
                if (e.GameCollectionModified || e.GameInfoModified || e.JumpListModified)
                    RefreshJumpList();

                return Task.CompletedTask;
            };
            Services.InstanceData.CultureChanged += (s, e) => RefreshJumpList();

            // Subscribe to when the app has finished setting up
            StartupComplete += App_StartupComplete_GameFinder_Async;
            StartupComplete += App_StartupComplete_Miscellaneous_Async;

            LogStartupTime("BasicStartup: Check launch arguments");

            // Check for user level argument
            if (Services.InstanceData.Arguments.Contains("-ul"))
            {
                try
                {
                    string ul = Services.InstanceData.Arguments[Services.InstanceData.Arguments.FindItemIndex(x => x == "-ul") + 1];
                    Data.App_UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Setting user level from args");
                }
            }

            // NOTE: Starting with the updater 3.0.0 (available from 4.5.0) this is no longer used. It must however be maintained for legacy support (i.e. updating to version 4.5.0+ using an updater below 3.0.0)
            // Check for updater install argument
            if (Services.InstanceData.Arguments.Contains("-install"))
            {
                try
                {
                    FileSystemPath updateFile = Services.InstanceData.Arguments[Services.InstanceData.Arguments.FindItemIndex(x => x == "-install") + 1];
                    if (updateFile.FileExists)
                    {
                        updateFile.GetFileInfo().Delete();
                        Logger.Info("The updater was deleted");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Deleting updater");
                }
            }

            // Update the application path
            FileSystemPath appPath = Assembly.GetEntryAssembly()?.Location;

            if (appPath != Data.App_ApplicationPath)
            {
                Data.App_ApplicationPath = appPath;

                Logger.Info("The application path has been updated");
            }

            LogStartupTime("BasicStartup: Deploy files");

            // Deploy additional files
            await Services.App.DeployFilesAsync(false);

            LogStartupTime("BasicStartup: Check for first launch");

            // Show first launch info
            if (Data.App_IsFirstLaunch || 
                // Show if updated to version 12 due to it having been changed
                Data.App_LastVersion < new Version(12, 0, 0, 2))
            {
                // Close the splash screen
                CloseSplashScreen();

                new FirstLaunchInfoDialog().ShowDialog();
                Data.App_IsFirstLaunch = false;
            }

            LogStartupTime("BasicStartup: Validating games");

            // Validate the added games
            await ValidateGamesAsync();

            LogStartupTime("BasicStartup: Finished validating games");
        }

        /// <summary>
        /// Runs the post-update code
        /// </summary>
        private async Task PostUpdateAsync()
        {
            if (Data.App_LastVersion < new Version(4, 0, 0, 6))
                Data.UI_EnableAnimations = true;

            if (Data.App_LastVersion < new Version(4, 1, 1, 0))
                Data.App_ShowIncompleteTranslations = false;

            if (Data.App_LastVersion < new Version(4, 5, 0, 0))
            {
                Data.UI_LinkItemStyle = UserData_LinkItemStyle.List;
                Data.App_ApplicationPath = Assembly.GetEntryAssembly()?.Location;
                Data.Update_ForceUpdate = false;
                Data.Update_GetBetaUpdates = false;
            }

            if (Data.App_LastVersion < new Version(4, 6, 0, 0))
                Data.UI_LinkListHorizontalAlignment = HorizontalAlignment.Left;

            if (Data.App_LastVersion < new Version(5, 0, 0, 0))
            {
                Data.Backup_CompressBackups = true;
                Data.Game_FiestaRunVersion = UserData_FiestaRunEdition.Default;

                // Due to the fiesta run version system being changed the game has to be removed and then re-added
                Data.Game_Games.Remove(Games.RaymanFiestaRun);

                // If a Fiesta Run backup exists the name needs to change to the new standard
                var fiestaBackupDir = Data.Backup_BackupLocation + AppViewModel.BackupFamily + "Rayman Fiesta Run";

                if (fiestaBackupDir.DirectoryExists)
                {
                    try
                    {
                        // Read the app data file
                        JObject appData = new StringReader(File.ReadAllText(AppFilePaths.AppUserDataPath)).RunAndDispose(x =>
                            new JsonTextReader(x).RunAndDispose(y => JsonSerializer.Create().Deserialize(y))).CastTo<JObject>();

                        // Get the previous Fiesta Run version
                        var isWin10 = appData["IsFiestaRunWin10Edition"].Value<bool>();

                        // Set the current edition
                        Data.Game_FiestaRunVersion = isWin10 ? UserData_FiestaRunEdition.Win10 : UserData_FiestaRunEdition.Default;

                        Services.File.MoveDirectory(fiestaBackupDir, Data.Backup_BackupLocation + AppViewModel.BackupFamily + Games.RaymanFiestaRun.GetGameInfo().BackupName, true, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Moving Fiesta Run backups to 5.0.0 standard");

                        await Services.MessageUI.DisplayMessageAsync(Metro.Resources.PostUpdate_MigrateFiestaRunBackup5Error, Metro.Resources.PostUpdate_MigrateBackupErrorHeader, MessageType.Error);
                    }
                }

                // Remove old temp dir
                try
                {
                    Services.File.DeleteDirectory(Path.Combine(Path.GetTempPath(), "RCP_Metro"));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Cleaning pre-5.0.0 temp");
                }

                Data.Update_DisableDowngradeWarning = false;
            }

            if (Data.App_LastVersion < new Version(6, 0, 0, 0))
            {
                Data.Game_EducationalDosBoxGames = null;
                Data.Game_RRR2LaunchMode = UserData_RRR2LaunchMode.AllGames;
                Data.Game_RabbidsGoHomeLaunchData = null;
            }

            if (Data.App_LastVersion < new Version(6, 0, 0, 2))
            {
                // By default, add all games to the jump list collection
                Data.App_JumpListItemIDCollection = Services.App.GetGames.
                    Where(x => x.IsAdded()).
                    Select(x => x.GetManager().GetJumpListItems().Select(y => y.ID)).
                    SelectMany(x => x).
                    ToList();
            }

            if (Data.App_LastVersion < new Version(7, 0, 0, 0))
            {
                Data.Update_IsUpdateAvailable = false;

                if (Data.App_UserLevel == UserLevel.Normal)
                    Data.App_UserLevel = UserLevel.Advanced;
            }

            if (Data.App_LastVersion < new Version(7, 1, 0, 0))
                Data.Game_InstalledGames = new HashSet<Games>();

            if (Data.App_LastVersion < new Version(7, 1, 1, 0))
                Data.UI_CategorizeGames = true;

            if (Data.App_LastVersion < new Version(7, 2, 0, 0))
                Data.Game_ShownRabbidsActivityCenterLaunchMessage = false;

            if (Data.App_LastVersion < new Version(9, 0, 0, 0))
            {
                const string regUninstallKeyName = "RCP_Metro";

                // Since support has been removed for showing the program under installed programs we now have to remove the key
                var keyPath = RegistryHelpers.CombinePaths(CommonRegistryPaths.InstalledPrograms, regUninstallKeyName);

                // Check if the key exists
                if (RegistryHelpers.KeyExists(keyPath))
                {
                    // Make sure the user is running as admin
                    if (Services.App.IsRunningAsAdmin)
                    {
                        try
                        {
                            // Open the parent key
                            using var parentKey = RegistryHelpers.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms, RegistryView.Default, true);

                            // Delete the sub-key
                            parentKey.DeleteSubKey(regUninstallKeyName);

                            Logger.Info("The program Registry key has been deleted");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Removing uninstall Registry key");

                            await Services.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                        }
                    }
                    else
                    {
                        await Services.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                    }
                }

                if (Data.Utility_TPLSData != null)
                {
                    Data.Utility_TPLSData.IsEnabled = false;
                    await Services.MessageUI.DisplayMessageAsync(Metro.Resources.PostUpdate_TPLSUpdatePrompt);
                }
            }

            if (Data.App_LastVersion < new Version(9, 4, 0, 0))
            {
                Data.Archive_GF_GenerateMipmaps = true;
                Data.Archive_GF_UpdateTransparency = UserData_Archive_GF_TransparencyMode.PreserveFormat;
            }

            if (Data.App_LastVersion < new Version(9, 5, 0, 0))
                Data.Binary_BinarySerializationFileLogPath = FileSystemPath.EmptyPath;

            if (Data.App_LastVersion < new Version(10, 0, 0, 0))
            {
                Data.Theme_SyncTheme = false;
                Data.App_HandleDownloadsManually = false;
            }

            if (Data.App_LastVersion < new Version(10, 2, 0, 0))
                Data.Archive_GF_ForceGF8888Import = false;

            if (Data.App_LastVersion < new Version(11, 0, 0, 0))
                Data.Archive_ExplorerSortOption = UserData_Archive_Sort.Default;

            if (Data.App_LastVersion < new Version(11, 1, 0, 0))
            {
                Data.Archive_BinaryEditorExe = FileSystemPath.EmptyPath;
                Data.Archive_AssociatedPrograms = new Dictionary<string, FileSystemPath>();
            }

            if (Data.App_LastVersion < new Version(11, 3, 0, 0))
                Data.Mod_RRR_KeyboardButtonMapping = new Dictionary<int, Key>();

            if (Data.App_LastVersion < new Version(12, 0, 0, 0))
            {
                Data.App_DisableGameValidation = false;
                Data.UI_UseChildWindows = true;
            }

            // Re-deploy files
            await Services.App.DeployFilesAsync(true);

            // Refresh the jump list
            RefreshJumpList();

            // Close the splash screen
            CloseSplashScreen();

            // Show app news
            await Services.DialogBaseManager.ShowWindowAsync(new AppNewsDialog());
        }

        /// <summary>
        /// Validates the added games
        /// </summary>
        /// <returns></returns>
        private static async Task ValidateGamesAsync()
        {
            // Keep track of removed games
            var removed = new List<Games>();

            // Make sure every game is valid
            foreach (var game in Services.App.GetGames)
            {
                // Check if it has been added
                if (!game.IsAdded())
                    continue;

                // Check if it's valid
                if (await game.GetManager().IsValidAsync(game.GetInstallDir()))
                    continue;

                // Show message
                await Services.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.GameNotFound, game.GetGameInfo().DisplayName), Metro.Resources.GameNotFoundHeader, MessageType.Error);

                // Remove the game from app data
                await Services.App.RemoveGameAsync(game, true);

                // Add to removed games
                removed.Add(game);

                Logger.Info("The game {0} has been removed due to not being valid", game);
            }

            // Refresh if any games were removed
            if (removed.Any())
                await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(removed, RefreshFlags.GameCollection));
        }

        private void InitializeLogging(string[] args)
        {
            // Create a new logging configuration
            var logConfig = new LoggingConfiguration();

#if DEBUG
            // On debug we default it to log trace
            LogLevel logLevel = LogLevel.Trace;
#else
            // If not on debug we default to log info
            LogLevel logLevel = LogLevel.Info;
#endif

            // Allow the log level to be specified from a launch argument
            if (args.Contains("-loglevel"))
            {
                string argLogLevel = args[args.FindItemIndex(x => x == "-loglevel") + 1];
                logLevel = LogLevel.FromString(argLogLevel);
            }

            const string logLayout = "${time:invariant=true}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}";
            bool logToFile = !args.Contains("-nofilelog");
            bool logToMemory = !args.Contains("-nomemlog");
            bool logToViewer = args.Contains("-logviewer");

            // Log to file
            if (logToFile)
            {
                logConfig.AddRule(logLevel, LogLevel.Fatal, new FileTarget("file")
                {
                    // Archive a maximum of 5 logs. This makes it easier going back to check errors which happened on older instances of the app.
                    ArchiveOldFileOnStartup = true,
                    ArchiveFileName = AppFilePaths.ArchiveLogFile.FullPath,
                    MaxArchiveFiles = 5,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    
                    // Keep the file open and disable concurrent writes to improve performance
                    KeepFileOpen = true,
                    ConcurrentWrites = false,

                    // Set the file path and layout
                    FileName = AppFilePaths.LogFile.FullPath,
                    Layout = logLayout,
                });
            }

            if (logToMemory)
            {
                logConfig.AddRule(logLevel, LogLevel.Fatal, new MemoryTarget("memory")
                {
                    Layout = logLayout,
                });
            }

            // Log to log viewer
            if (logToViewer)
            {
                LogViewerViewModel = new LogViewerViewModel();

                // Always log from trace to fatal to include all logs
                logConfig.AddRuleForAllLevels(new MethodCallTarget("logviewer", async (logEvent, _) =>
                {
                    // Await to avoid blocking
                    await Dispatcher.InvokeAsync(() =>
                    {
                        var log = new LogItemViewModel(logEvent.Level, logEvent.Exception, logEvent.TimeStamp, logEvent.LoggerName, logEvent.FormattedMessage);
                        log.IsVisible = log.LogLevel >= LogViewerViewModel.ShowLogLevel;
                        LogViewerViewModel.LogItems.Add(log);
                    });
                }));
            }

            // Apply config
            LogManager.Configuration = logConfig;
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
            await Services.App.EnableUbiIniWriteAccessAsync();
        }

        private static async Task App_StartupComplete_GameFinder_Async(object sender, EventArgs eventArgs)
        {
            // Check for installed games
            if (Services.Data.Game_AutoLocateGames)
                await Services.App.RunGameFinderAsync();
        }

        private static async Task App_StartupComplete_Updater_Async(object sender, EventArgs eventArgs)
        {
            if (AppFilePaths.UpdaterFilePath.FileExists)
            {
                int retryTime = 0;

                // Wait until we can write to the file (i.e. it closing after an update)
                while (!Services.File.CheckFileWriteAccess(AppFilePaths.UpdaterFilePath))
                {
                    retryTime++;

                    // Try for 2 seconds first
                    if (retryTime < 20)
                    {
                        Logger.Debug("The updater can not be removed due to not having write access. Retrying {0}", retryTime);

                        await Task.Delay(100);
                    }
                    // Now it's taking a long time... Try for 10 more seconds
                    else if (retryTime < 70)
                    {
                        Logger.Warn("The updater can not be removed due to not having write access. Retrying {0}", retryTime);

                        await Task.Delay(200);
                    }
                    // Give up and let the deleting of the file give an error message
                    else
                    {
                        Logger.Fatal("The updater can not be removed due to not having write access");
                        break;
                    }
                }

                try
                {
                    // Remove the updater
                    Services.File.DeleteFile(AppFilePaths.UpdaterFilePath);

                    Logger.Info("The updater has been removed");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Removing updater");
                }
            }

            // Check for updates
            if (Services.Data.Update_AutoUpdate)
                await Services.App.CheckForUpdatesAsync(false);
        }

        private Task BaseApp_StartupComplete_Miscellaneous_Async(object sender, EventArgs eventArgs)
        {
            if (Dispatcher == null)
                throw new Exception("Dispatcher is null");

            // Run on UI thread
            Dispatcher.Invoke(() =>
            {
                // Show log viewer if available
                if (IsLogViewerAvailable)
                {
                    LogViewer.Open();
                    MainWindow?.Focus();
                }
            });

            return Task.CompletedTask;
        }

        private static void App_StartupEventsCompleted(object sender, EventArgs e)
        {
            Services.App.IsStartupRunning = false;
        }

        private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            using (await DataChangedHandlerAsyncLock.LockAsync())
            {
                switch (e.PropertyName)
                {
                    case nameof(AppUserData.Theme_DarkMode):
                    case nameof(AppUserData.Theme_SyncTheme):
                        this.SetTheme(Data.Theme_DarkMode, Data.Theme_SyncTheme);
                        break;

                    case nameof(AppUserData.Backup_BackupLocation):

                        await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.Backups));

                        if (!PreviousBackupLocation.DirectoryExists)
                        {
                            Logger.Info("The backup location has been changed, but the previous directory does not exist");
                            return;
                        }

                        Logger.Info("The backup location has been changed and old backups are being moved...");

                        await Services.App.MoveBackupsAsync(PreviousBackupLocation, Data.Backup_BackupLocation);

                        PreviousBackupLocation = Data.Backup_BackupLocation;

                        break;

                    case nameof(AppUserData.UI_LinkItemStyle):
                        static string GetStyleSource(UserData_LinkItemStyle linkItemStye) => $"{AppViewModel.WPFApplicationBasePath}/UI/Resources/Styles.LinkItem.{linkItemStye}.xaml";

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
                            Source = new Uri(GetStyleSource(Data.UI_LinkItemStyle))
                        });

                        PreviousLinkItemStyle = Data.UI_LinkItemStyle;

                        break;

                    case nameof(AppUserData.Game_RRR2LaunchMode):
                        await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanRavingRabbids2, RefreshFlags.GameInfo));
                        break;

                    case nameof(AppUserData.Emu_DOSBox_Path):
                    case nameof(AppUserData.Emu_DOSBox_ConfigPath):
                        await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.GameInfo));
                        break;
                }
            }
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
                    if (Services.Data.App_JumpListItemIDCollection == null)
                    {
                        Logger.Warn("The jump could not refresh due to collection not existing");

                        return;
                    }

                    // Create a jump list
                    new JumpList(Services.App.GetGames.
                            // Add only games which have been added
                            Where(x => x.IsAdded()).
                            // Get the items for each game
                            Select(x => x.GetManager().GetJumpListItems()).
                            // Select into single collection
                            SelectMany(x => x).
                            // Keep only the included items
                            Where(x => x.IsIncluded).
                            // Keep custom order
                            OrderBy(x => Services.Data.App_JumpListItemIDCollection.IndexOf(x.ID)).
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

                    Logger.Info("The jump list has been refreshed");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Creating jump list");
                }
            });
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The app user data
        /// </summary>
        private AppUserData Data { get; set; }

        /// <summary>
        /// The async lock for <see cref="Data_PropertyChangedAsync"/>
        /// </summary>
        private AsyncLock DataChangedHandlerAsyncLock { get; }

        /// <summary>
        /// The saved previous backup location
        /// </summary>
        private FileSystemPath PreviousBackupLocation { get; set; }

        /// <summary>
        /// The saved previous link item style
        /// </summary>
        private UserData_LinkItemStyle PreviousLinkItemStyle { get; set; }

        #endregion

        #region Public Properties

        public bool IsLogViewerAvailable => LogViewerViewModel != null;
        public LogViewerViewModel LogViewerViewModel { get; set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion
    }
}