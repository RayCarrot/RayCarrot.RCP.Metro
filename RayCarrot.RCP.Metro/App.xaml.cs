using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Extensions.DependencyInjection;
using RayCarrot.Logging;

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

        #region Protected Overrides

        /// <summary>
        /// Gets the services to use for the application
        /// </summary>
        /// <param name="logLevel">The level to log</param>
        /// <param name="args">The launch arguments</param>
        /// <returns>The services to use</returns>
        protected override IServiceCollection GetServices(LogLevel logLevel, string[] args)
        {
            // Set file log level
            FileLogger.FileLoggerLogLevel = logLevel;

            // Add services
            return new ServiceCollection().
                // Add loggers
                AddLoggers(DefaultLoggers.Console | DefaultLoggers.Debug | DefaultLoggers.Session, logLevel, builder => builder.AddProvider(new BaseLogProvider<FileLogger>())).
                // Add user data
                AddSingleton(new AppUserData()).
                // Add exception handler
                AddExceptionHandler<RCPExceptionHandler>().
                // Add message UI manager
                AddMessageUIManager<RCPMessageUIManager>().
                // Add browse UI manager
                AddBrowseUIManager<DefaultWPFBrowseUIManager>().
                // Add file manager
                AddFileManager<RCPFileManager>().
                // Add dialog base manager
                AddDialogBaseManager<RCPDialogBaseManager>().
                // Add update manager
                AddUpdateManager<RCPUpdateManager>().
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
                if (CommonPaths.AppUserDataPath.FileExists)
                {
                    JsonConvert.PopulateObject(File.ReadAllText(CommonPaths.AppUserDataPath), RCPServices.Data);
                    RL.Logger?.LogInformationSource($"The app user data has been loaded");

                    // Verify the data
                    RCPServices.Data.Verify();
                }
                else
                {
                    // Reset the user data
                    RCPServices.Data.Reset();

                    RL.Logger?.LogInformationSource($"The app user data has been reset");
                }
            }
            catch (Exception ex)
            {
                ex.HandleError($"Loading app user data");

                // NOTE: This is not localized due to the current culture not having been set at this point
                MessageBox.Show($"An error occurred reading saved app data. Some settings have been reset to their default values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Reset the user data
                RCPServices.Data.Reset();
            }

            Data = RCPServices.Data;

            LogStartupTime("Setup: Setting theme");

            // Set the theme
            this.SetTheme(Data.DarkMode, Data.SyncTheme);

            LogStartupTime("Setup: Setting culture");

            // Apply the current culture if defaulted
            if (Data.CurrentCulture == LocalizationManager.DefaultCulture.Name)
                LocalizationManager.SetCulture(LocalizationManager.DefaultCulture.Name);

            LogStartupTime("Setup: Setup WPF trace listener");

            // Listen to data binding logs
            WPFTraceListener.Setup(LogLevel.Warning);

            StartupComplete += BaseApp_StartupComplete_Miscellaneous_Async;
            StartupComplete += App_StartupComplete_Updater_Async;
            Data.PropertyChanged += Data_PropertyChangedAsync;

            // Run basic startup
            await BasicStartupAsync();

            RL.Logger?.LogInformationSource($"Current version is {RCPServices.App.CurrentAppVersion}");

            // Check if it's a new version
            if (Data.LastVersion < RCPServices.App.CurrentAppVersion)
            {
                // Run post-update code
                await PostUpdateAsync();

                LogStartupTime("Setup: Post update has run");

                // Update the last version
                Data.LastVersion = RCPServices.App.CurrentAppVersion;
            }
            // Check if it's a lower version than previously recorded
            else if (Data.LastVersion > RCPServices.App.CurrentAppVersion)
            {
                RL.Logger?.LogWarningSource($"A newer version ({Data.LastVersion}) has been recorded in the application data");

                if (!Data.DisableDowngradeWarning)
                    await Services.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.DowngradeWarning, RCPServices.App.CurrentAppVersion,
                        Data.LastVersion), Metro.Resources.DowngradeWarningHeader, MessageType.Warning);
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
            RCPServices.Data?.WindowState?.ApplyToWindow(window);

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
            if (RCPServices.Data == null)
                return;
            
            // Save window state
            RCPServices.Data.WindowState = WindowSessionState.GetWindowState(mainWindow);

            RL.Logger?.LogInformationSource($"The application is exiting...");

            // Save all user data
            await RCPServices.App.SaveUserDataAsync();
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
            if (AppViewModel.WindowsVersion < WindowsVersion.WinVista)
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

        /// <summary>
        /// Disposes any disposable application objects
        /// </summary>
        protected override void Dispose()
        {
            // Dispose base
            base.Dispose();

            // Dispose log file
            AppLogFileStream?.Dispose();
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
                int regValue = Registry.GetValue(CommonPaths.RegistryBaseKey, CommonPaths.RegistryLicenseValue, 0)?.CastTo<int>() ?? 0;

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
                    Registry.SetValue(CommonPaths.RegistryBaseKey, CommonPaths.RegistryLicenseValue, 1);

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
            PreviousLinkItemStyle = Data.LinkItemStyle;
            PreviousBackupLocation = Data.BackupLocation;

            // Subscribe to when to refresh the jump list
            RCPServices.App.RefreshRequired += (_, e) =>
            {
                if (e.GameCollectionModified || e.GameInfoModified || e.JumpListModified)
                    RefreshJumpList();

                return Task.CompletedTask;
            };
            Services.Data.CultureChanged += (s, e) => RefreshJumpList();

            // Subscribe to when the app has finished setting up
            StartupComplete += App_StartupComplete_GameFinder_Async;
            StartupComplete += App_StartupComplete_Miscellaneous_Async;

            LogStartupTime("BasicStartup: Check launch arguments");

            // Check for reset argument
            if (Services.Data.Arguments.Contains("-reset"))
                RCPServices.Data.Reset();

            // Check for user level argument
            if (Services.Data.Arguments.Contains("-ul"))
            {
                try
                {
                    string ul = Services.Data.Arguments[Services.Data.Arguments.FindItemIndex(x => x == "-ul") + 1];
                    Data.UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Setting user level from args");
                }
            }

            // NOTE: Starting with the updater 3.0.0 (available from 4.5.0) this is no longer used. It must however be maintained for legacy support (i.e. updating to version 4.5.0+ using an updater below 3.0.0)
            // Check for updater install argument
            if (Services.Data.Arguments.Contains("-install"))
            {
                try
                {
                    FileSystemPath updateFile = Services.Data.Arguments[Services.Data.Arguments.FindItemIndex(x => x == "-install") + 1];
                    if (updateFile.FileExists)
                    {
                        updateFile.GetFileInfo().Delete();
                        RL.Logger?.LogInformationSource($"The updater was deleted");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Deleting updater");
                }
            }

            // Update the application path
            FileSystemPath appPath = Assembly.GetEntryAssembly()?.Location;

            if (appPath != Data.ApplicationPath)
            {
                Data.ApplicationPath = appPath;

                RL.Logger?.LogInformationSource("The application path has been updated");
            }

            LogStartupTime("BasicStartup: Deploy files");

            // Deploy additional files
            await RCPServices.App.DeployFilesAsync(false);

            LogStartupTime("BasicStartup: Check for first launch");

            // Show first launch info
            if (Data.IsFirstLaunch)
            {
                // Close the splash screen
                CloseSplashScreen();

                new FirstLaunchInfoDialog().ShowDialog();
                Data.IsFirstLaunch = false;
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
            if (Data.LastVersion < new Version(4, 0, 0, 6))
                Data.EnableAnimations = true;

            if (Data.LastVersion < new Version(4, 1, 1, 0))
                Data.ShowIncompleteTranslations = false;

            if (Data.LastVersion < new Version(4, 5, 0, 0))
            {
                Data.LinkItemStyle = UserData_LinkItemStyle.List;
                Data.ApplicationPath = Assembly.GetEntryAssembly()?.Location;
                Data.ForceUpdate = false;
                Data.GetBetaUpdates = false;
            }

            if (Data.LastVersion < new Version(4, 6, 0, 0))
                Data.LinkListHorizontalAlignment = HorizontalAlignment.Left;

            if (Data.LastVersion < new Version(5, 0, 0, 0))
            {
                Data.CompressBackups = true;
                Data.FiestaRunVersion = UserData_FiestaRunEdition.Default;

                // Due to the fiesta run version system being changed the game has to be removed and then re-added
                Data.Games.Remove(Games.RaymanFiestaRun);

                // If a Fiesta Run backup exists the name needs to change to the new standard
                var fiestaBackupDir = Data.BackupLocation + AppViewModel.BackupFamily + "Rayman Fiesta Run";

                if (fiestaBackupDir.DirectoryExists)
                {
                    try
                    {
                        // Read the app data file
                        JObject appData = new StringReader(File.ReadAllText(CommonPaths.AppUserDataPath)).RunAndDispose(x =>
                            new JsonTextReader(x).RunAndDispose(y => JsonSerializer.Create().Deserialize(y))).CastTo<JObject>();

                        // Get the previous Fiesta Run version
                        var isWin10 = appData["IsFiestaRunWin10Edition"].Value<bool>();

                        // Set the current edition
                        Data.FiestaRunVersion = isWin10 ? UserData_FiestaRunEdition.Win10 : UserData_FiestaRunEdition.Default;

                        RCPServices.File.MoveDirectory(fiestaBackupDir, Data.BackupLocation + AppViewModel.BackupFamily + Games.RaymanFiestaRun.GetGameInfo().BackupName, true, true);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Moving Fiesta Run backups to 5.0.0 standard");

                        await Services.MessageUI.DisplayMessageAsync(Metro.Resources.PostUpdate_MigrateFiestaRunBackup5Error, Metro.Resources.PostUpdate_MigrateBackupErrorHeader, MessageType.Error);
                    }
                }

                // Remove old temp dir
                try
                {
                    RCPServices.File.DeleteDirectory(Path.Combine(Path.GetTempPath(), "RCP_Metro"));
                }
                catch (Exception ex)
                {
                    ex.HandleError("Cleaning pre-5.0.0 temp");
                }

                Data.DisableDowngradeWarning = false;
            }

            if (Data.LastVersion < new Version(6, 0, 0, 0))
            {
                Data.EducationalDosBoxGames = null;
                Data.RRR2LaunchMode = UserData_RRR2LaunchMode.AllGames;
                Data.RabbidsGoHomeLaunchData = null;
            }

            if (Data.LastVersion < new Version(6, 0, 0, 2))
            {
                // By default, add all games to the jump list collection
                Data.JumpListItemIDCollection = RCPServices.App.GetGames.
                    Where(x => x.IsAdded()).
                    Select(x => x.GetManager().GetJumpListItems().Select(y => y.ID)).
                    SelectMany(x => x).
                    ToList();
            }

            if (Data.LastVersion < new Version(7, 0, 0, 0))
            {
                Data.IsUpdateAvailable = false;

                if (Data.UserLevel == UserLevel.Normal)
                    Data.UserLevel = UserLevel.Advanced;
            }

            if (Data.LastVersion < new Version(7, 1, 0, 0))
                Data.InstalledGames = new HashSet<Games>();

            if (Data.LastVersion < new Version(7, 1, 1, 0))
                Data.CategorizeGames = true;

            if (Data.LastVersion < new Version(7, 2, 0, 0))
                Data.ShownRabbidsActivityCenterLaunchMessage = false;

            if (Data.LastVersion < new Version(9, 0, 0, 0))
            {
                const string regUninstallKeyName = "RCP_Metro";

                // Since support has been removed for showing the program under installed programs we now have to remove the key
                var keyPath = RegistryHelpers.CombinePaths(CommonRegistryPaths.InstalledPrograms, regUninstallKeyName);

                // Check if the key exists
                if (RegistryHelpers.KeyExists(keyPath))
                {
                    // Make sure the user is running as admin
                    if (RCPServices.App.IsRunningAsAdmin)
                    {
                        try
                        {
                            // Open the parent key
                            using var parentKey = RegistryHelpers.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms, RegistryView.Default, true);

                            // Delete the sub-key
                            parentKey.DeleteSubKey(regUninstallKeyName);

                            RL.Logger?.LogInformationSource("The program Registry key has been deleted");
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Removing uninstall Registry key");

                            await Services.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                        }
                    }
                    else
                    {
                        await Services.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                    }
                }

                if (Data.TPLSData != null)
                {
                    Data.TPLSData.IsEnabled = false;
                    await Services.MessageUI.DisplayMessageAsync(Metro.Resources.PostUpdate_TPLSUpdatePrompt);
                }
            }

            if (Data.LastVersion < new Version(9, 4, 0, 0))
            {
                Data.Archive_GF_GenerateMipmaps = true;
                Data.Archive_GF_UpdateTransparency = UserData_Archive_GF_TransparencyMode.PreserveFormat;
            }

            if (Data.LastVersion < new Version(9, 5, 0, 0))
                Data.BinarySerializationFileLogPath = FileSystemPath.EmptyPath;

            if (Data.LastVersion < new Version(10, 0, 0, 0))
            {
                Data.SyncTheme = false;
                Data.HandleDownloadsManually = false;
            }

            if (Data.LastVersion < new Version(10, 2, 0, 0))
                Data.Archive_GF_ForceGF8888Import = false;

            if (Data.LastVersion < new Version(11, 0, 0, 0))
                Data.ArchiveExplorerSortOption = UserData_Archive_Sort.Default;

            if (Data.LastVersion < new Version(11, 1, 0, 0))
            {
                Data.Archive_BinaryEditorExe = FileSystemPath.EmptyPath;
                Data.Archive_AssociatedPrograms = new Dictionary<string, FileSystemPath>();
            }

            if (Data.LastVersion < new Version(11, 3, 0, 0))
                Data.Mod_RRR_KeyboardButtonMapping = new Dictionary<int, Key>();

            // Re-deploy files
            await RCPServices.App.DeployFilesAsync(true);

            // Refresh the jump list
            RefreshJumpList();

            // Close the splash screen
            CloseSplashScreen();

            // Show app news
            new AppNewsDialog().ShowDialog();
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
            foreach (var game in RCPServices.App.GetGames)
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
                await RCPServices.App.RemoveGameAsync(game, true);

                // Add to removed games
                removed.Add(game);

                RL.Logger?.LogInformationSource($"The game {game} has been removed due to not being valid");
            }

            // Refresh if any games were removed
            if (removed.Any())
                await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(removed, true, false, false, false));
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
            await RCPServices.App.EnableUbiIniWriteAccessAsync();
        }

        private static async Task App_StartupComplete_GameFinder_Async(object sender, EventArgs eventArgs)
        {
            // Check for installed games
            if (RCPServices.Data.AutoLocateGames)
                await RCPServices.App.RunGameFinderAsync();
        }

        private static async Task App_StartupComplete_Updater_Async(object sender, EventArgs eventArgs)
        {
            if (CommonPaths.UpdaterFilePath.FileExists)
            {
                int retryTime = 0;

                // Wait until we can write to the file (i.e. it closing after an update)
                while (!RCPServices.File.CheckFileWriteAccess(CommonPaths.UpdaterFilePath))
                {
                    retryTime++;

                    // Try for 2 seconds first
                    if (retryTime < 20)
                    {
                        RL.Logger?.LogDebugSource($"The updater can not be removed due to not having write access. Retrying {retryTime}");

                        await Task.Delay(100);
                    }
                    // Now it's taking a long time... Try for 10 more seconds
                    else if (retryTime < 70)
                    {
                        RL.Logger?.LogWarningSource($"The updater can not be removed due to not having write access. Retrying {retryTime}");

                        await Task.Delay(200);
                    }
                    // Give up and let the deleting of the file give an error message
                    else
                    {
                        RL.Logger?.LogCriticalSource($"The updater can not be removed due to not having write access");
                        break;
                    }
                }

                try
                {
                    // Remove the updater
                    RCPServices.File.DeleteFile(CommonPaths.UpdaterFilePath);

                    RL.Logger?.LogInformationSource($"The updater has been removed");
                }
                catch (Exception ex)
                {
                    ExceptionExtensions.HandleCritical(ex, "Removing updater");
                }
            }

            // Check for updates
            if (RCPServices.Data.AutoUpdate)
                await RCPServices.App.CheckForUpdatesAsync(false);
        }

        private async Task BaseApp_StartupComplete_Miscellaneous_Async(object sender, EventArgs eventArgs)
        {
            if (Dispatcher == null)
                throw new Exception("Dispatcher is null");

            // Run on UI thread
            await Dispatcher.Invoke(async () =>
            {
                // Show log viewer if a debugger is attached
                if (Debugger.IsAttached)
                {
                    var logViewer = new LogViewer();
                    var win = await logViewer.ShowWindowAsync();

                    // NOTE: This is a temporary solution to avoid the log viewer blocking the main window
                    win.Owner = null;
                    win.ShowInTaskbar = true;
                    MainWindow?.Focus();
                }
            });
        }

        private static void App_StartupEventsCompleted(object sender, EventArgs e)
        {
            RCPServices.App.IsStartupRunning = false;
        }

        private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            using (await DataChangedHandlerAsyncLock.LockAsync())
            {
                switch (e.PropertyName)
                {
                    case nameof(AppUserData.DarkMode):
                    case nameof(AppUserData.SyncTheme):
                        this.SetTheme(Data.DarkMode, Data.SyncTheme);
                        break;

                    case nameof(AppUserData.BackupLocation):

                        await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, true, false));

                        if (!PreviousBackupLocation.DirectoryExists)
                        {
                            RL.Logger?.LogInformationSource("The backup location has been changed, but the previous directory does not exist");
                            return;
                        }

                        RL.Logger?.LogInformationSource("The backup location has been changed and old backups are being moved...");

                        await RCPServices.App.MoveBackupsAsync(PreviousBackupLocation, Data.BackupLocation);

                        PreviousBackupLocation = Data.BackupLocation;

                        break;

                    case nameof(AppUserData.LinkItemStyle):
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
                            Source = new Uri(GetStyleSource(Data.LinkItemStyle))
                        });

                        PreviousLinkItemStyle = Data.LinkItemStyle;

                        break;

                    case nameof(AppUserData.RRR2LaunchMode):
                        await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanRavingRabbids2, false, false, false, true));
                        break;

                    case nameof(AppUserData.DosBoxPath):
                    case nameof(AppUserData.DosBoxConfig):
                        await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, false, true));
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
                    if (RCPServices.Data.JumpListItemIDCollection == null)
                    {
                        RL.Logger?.LogWarningSource("The jump could not refresh due to collection not existing");

                        return;
                    }

                    // Create a jump list
                    new JumpList(RCPServices.App.GetGames.
                            // Add only games which have been added
                            Where(x => x.IsAdded()).
                            // Get the items for each game
                            Select(x => x.GetManager().GetJumpListItems()).
                            // Select into single collection
                            SelectMany(x => x).
                            // Keep only the included items
                            Where(x => x.IsIncluded).
                            // Keep custom order
                            OrderBy(x => RCPServices.Data.JumpListItemIDCollection.IndexOf(x.ID)).
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

                    RL.Logger?.LogInformationSource("The jump list has been refreshed");
                }
                catch (Exception ex)
                {
                    ExceptionExtensions.HandleError(ex, "Creating jump list");
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

        /// <summary>
        /// The file stream for the application log file
        /// </summary>
        public StreamWriter AppLogFileStream { get; set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion
    }
}