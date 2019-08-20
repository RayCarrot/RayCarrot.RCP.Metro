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
using ByteSizeLib;
using MahApps.Metro;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseRCFApp
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public App() : base(true)
        {
            DataChangedHandlerAsyncLock = new AsyncLock();
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected override Window GetMainWindow()
        {
            // Create the window
            var window = new MainWindow();

            // Load previous state
            if (RCFRCP.Data.WindowState != null)
                RCFRCP.Data.WindowState.ApplyToWindow(window);

            return window;
        }

        /// <summary>
        /// Sets up the framework with loggers and other services
        /// </summary>
        /// <param name="construction">The construction</param>
        /// <param name="logLevel">The level to log</param>
        /// <param name="args">The launch arguments</param>
        protected override void SetupFramework(IFrameworkConstruction construction, LogLevel logLevel, string[] args)
        {
            // Set file log level
            FileLogger.FileLoggerLogLevel = logLevel;

            var loggers = DefaultLoggers.Console;

            // Only add debug loggers if a launch argument specifies it or if a debugger is attached
            if (Debugger.IsAttached || args.Contains("-debugLoggers"))
                loggers = loggers | DefaultLoggers.Debug | DefaultLoggers.Session;

            construction.
                // Add loggers
                AddLoggers(loggers, logLevel, builder => builder.AddProvider(new BaseLogProvider<FileLogger>())).
                // Add exception handler
                AddExceptionHandler<RCPExceptionHandler>().
                // Add user data manager
                AddUserDataManager(() => new JsonBaseSerializer()
                {
                    Formatting = Formatting.Indented
                }).
                // Add message UI manager
                AddMessageUIManager<RCPMessageUIManager>().
                // Add browse UI manager
                AddBrowseUIManager<RCPWPFBrowseUIManager>().
                // Add registry manager
                AddRegistryManager<DefaultRegistryManager>().
                // Add registry browse UI manager
                AddRegistryBrowseUIManager<DefaultWPFRegistryBrowseUIManager>().
                // Add the app view model
                AddSingleton(new AppViewModel()).
                // Add a file manager
                AddTransient<RCPFileManager>().
                // Add a dialog manager
                AddDialogBaseManager<RCPDialogBaseManager>().
                // Add Rayman defaults
                AddRaymanDefaults().
                // Add App UI manager
                AddTransient<AppUIManager>().
                // Add backup manager
                AddTransient<BackupManager>();
        }

        /// <summary>
        /// An optional custom setup to override
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>The task</returns>
        protected override async Task OnSetupAsync(string[] args)
        {
            LogStartupTime("Setup is starting");

            // Load the user data
            try
            {
                await RCFData.UserDataCollection.AddUserDataAsync<AppUserData>(CommonPaths.AppUserDataPath);
                RCFCore.Logger?.LogInformationSource($"The app user data has been loaded");
            }
            catch (Exception ex)
            {
                ex.HandleError($"Loading app user data");

                // NOTE: This is not localized due to the current culture not having been set at this point
                MessageBox.Show($"An error occurred reading saved app data. Some settings may be reset to their default values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Recreate the user data and reset it
                RCFData.UserDataCollection.Add(new AppUserData());
                RCFRCP.Data.Reset();
            }

            Data = RCFRCP.Data;

            LogStartupTime("User data has been loaded");

            // Track changes to the user data
            PreviousLinkItemStyle = Data.LinkItemStyle;
            PreviousBackupLocation = Data.BackupLocation;
            Data.PropertyChanged += Data_PropertyChangedAsync;

            // Apply the current culture if defaulted
            if (Data.CurrentCulture == AppLanguages.DefaultCulture.Name)
                Data.RefreshCulture(AppLanguages.DefaultCulture.Name);

            // Check if the program is shown under installed programs if the value is defaulted
            if (Data.ShowUnderInstalledPrograms == false)
                Data.PendingRegUninstallKeyRefresh = true;

            // Subscribe to when to refresh the jump list
            RCFRCP.App.RefreshRequired += (s, e) =>
            {
                if (e.GameCollectionModified || e.GameInfoModified)
                    RefreshJumpList();

                return Task.CompletedTask;
            };
            RCFCore.Data.CultureChanged += (s, e) => RefreshJumpList();

            // Listen to data binding logs
            WPFTraceListener.Setup(LogLevel.Warning);

            RCFCore.Logger?.LogInformationSource($"The temp directory has been created");

            // Run basic startup
            await BasicStartupAsync();

            LogStartupTime("Basic startup has run");

            // Run post-update code
            await PostUpdateAsync();

            LogStartupTime("Post update has run");

            // Check if a refresh is pending for the Registry uninstall key
            if (Data.PendingRegUninstallKeyRefresh)
                // If succeeded, remove the pending indicator
                if (await RCFRCP.Data.RefreshShowUnderInstalledProgramsAsync(Data.ShowUnderInstalledPrograms, true))
                    Data.PendingRegUninstallKeyRefresh = false;
        }

        /// <summary>
        /// An optional method to override which runs when closing
        /// </summary>
        /// <param name="mainWindow">The main Window of the application</param>
        /// <returns>The task</returns>
        protected override async Task OnCloseAsync(Window mainWindow)
        {
            // Save state
            Data.WindowState = WindowSessionState.GetWindowState(mainWindow);

            RCFCore.Logger?.LogInformationSource($"The application is exiting...");

            // Save all user data
            await RCFRCP.App.SaveUserDataAsync();
        }

        /// <summary>
        /// Override to run when another instance of the program is found running
        /// </summary>
        /// <param name="args">The launch arguments</param>
        protected override void OnOtherInstanceFound(string[] args)
        {
            MessageBox.Show("An instance of the Rayman Control Panel is already running", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Optional initial setup to run. Can be used to check if the environment is valid
        /// for the application to run or for the user to accept the license.
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>True if the setup finished successfully or false if the application has to shut down</returns>
        protected override Task<bool> InitialSetupAsync(string[] args)
        {
            // Make sure we are on Windows Vista or higher for the Windows API Code Pack and Deployment Image Servicing and Management
            if (AppViewModel.WindowsVersion < WindowsVersion.WinVista)
            {
                MessageBox.Show("Windows Vista or higher is required to run this application", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult(false);
            }

            // Make sure the license has been accepted
            if (!ShowLicense())
                return Task.FromResult(false);

            // Hard code the current directory
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory());

            // Attempt to remove log file if over 2 mb
            try
            {
                if (CommonPaths.LogFile.FileExists && CommonPaths.LogFile.GetSize() > ByteSize.FromMegaBytes(2))
                    File.Delete(CommonPaths.LogFile);
            }
            catch (Exception ex)
            {
                ex.HandleCritical("Removing log file due to size");
            }

            return Task.FromResult(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the application license message and returns a value indicating if it was accepted
        /// </summary>
        /// <returns>True if it was accepted, false if not</returns>
        private static bool ShowLicense()
        {
            try
            {
                // Get the license value, if one exists
                int regValue = Registry.GetValue(CommonPaths.RegistryBaseKey, CommonPaths.RegistryLicenseValue, 0)?.CastTo<int>() ?? 0;

                // Check if it has been accepted
                if (regValue == 1)
                    return true;

                // Create license popup dialog
                var ld = new LicenseDialog();

                // Show the dialog
                ld.ShowDialog();

                // Set Registry value if accepted
                if (ld.Accepted)
                    Registry.SetValue(CommonPaths.RegistryBaseKey, CommonPaths.RegistryLicenseValue, 1);

                // Return if it was accepted
                return ld.Accepted;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The license verification failed with the message of: {ex.Message}", "License error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Runs the basic startup
        /// </summary>
        /// <returns>The task</returns>
        private async Task BasicStartupAsync()
        {
            // Check for reset argument
            if (RCFCore.Data.Arguments.Contains("-reset"))
                RCFRCP.Data.Reset();

            // Check for user level argument
            if (RCFCore.Data.Arguments.Contains("-ul"))
            {
                try
                {
                    string ul = RCFCore.Data.Arguments[RCFCore.Data.Arguments.FindItemIndex(x => x == "-ul") + 1];
                    Data.UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Setting user level from args");
                }
            }

            // NOTE: Starting with the updater 3.0.0 (available from 4.5.0) this is no longer used. It must however be maintained for legacy support (i.e. updating to version 4.5.0+ using an updater below 3.0.0)
            // Check for updater install argument
            if (RCFCore.Data.Arguments.Contains("-install"))
            {
                try
                {
                    FileSystemPath updateFile = RCFCore.Data.Arguments[RCFCore.Data.Arguments.FindItemIndex(x => x == "-install") + 1];
                    if (updateFile.FileExists)
                    {
                        updateFile.GetFileInfo().Delete();
                        RCFCore.Logger?.LogInformationSource($"The updater was deleted");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Deleting updater");
                }
            }

            // Update the application path
            var appPath = Assembly.GetExecutingAssembly().Location;
            if (new FileSystemPath(appPath) != Data.ApplicationPath)
            {
                Data.ApplicationPath = appPath;

                RCFCore.Logger?.LogInformationSource("The application path has been updated");

                // Refresh Registry value to reflect new application path
                Data.PendingRegUninstallKeyRefresh = true;
            }

            await RCFRCP.App.DeployFilesAsync(false);

            // Show first launch info
            if (Data.IsFirstLaunch)
            {
                new FirstLaunchInfoDialog().ShowDialog();
                Data.IsFirstLaunch = false;
            }

            await RCFRCP.App.EnableUbiIniWriteAccessAsync();
        }

        /// <summary>
        /// Runs the post-update code
        /// </summary>
        private async Task PostUpdateAsync()
        {
            RCFCore.Logger?.LogInformationSource($"Current version is {RCFRCP.App.CurrentVersion}");

            // Make sure this is a new version
            if (!(Data.LastVersion < RCFRCP.App.CurrentVersion))
            {
                // Check if it's a lower version than previously recorded
                if (Data.LastVersion > RCFRCP.App.CurrentVersion)
                {
                    RCFCore.Logger?.LogWarningSource($"A newer version $({Data.LastVersion}) has been recorded in the application data");

                    if (!Data.DisableDowngradeWarning)
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.DowngradeWarning, RCFRCP.App.CurrentVersion, Data.LastVersion), Metro.Resources.DowngradeWarningHeader, MessageType.Warning);
                }

                return;
            }

            if (Data.LastVersion < new Version(4, 0, 0, 6))
                Data.EnableAnimations = true;

            if (Data.LastVersion < new Version(4, 1, 1, 0))
                Data.ShowIncompleteTranslations = false;

            if (RCFRCP.Data.LastVersion < new Version(4, 5, 0, 0))
            {
                Data.LinkItemStyle = LinkItemStyles.List;
                Data.ApplicationPath = Assembly.GetExecutingAssembly().Location;
                Data.ForceUpdate = false;
                Data.ShowUnderInstalledPrograms = false;
                Data.GetBetaUpdates = false;
            }

            if (Data.LastVersion < new Version(4, 6, 0, 0))
                Data.LinkListHorizontalAlignment = HorizontalAlignment.Left;

            if (Data.LastVersion < new Version(5, 0, 0, 0))
            {
                Data.CompressBackups = true;
                Data.FiestaRunVersion = FiestaRunEdition.Default;

                // Due to the fiesta run version system being changed the game has to be removed and then re-added
                Data.Games.Remove(Games.RaymanFiestaRun);

                // If a Fiesta Run backup exists the name needs to change to the new standard
                var fiestaBackupDir = RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily + "Rayman Fiesta Run";
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
                        Data.FiestaRunVersion = isWin10 ? FiestaRunEdition.Win10 : FiestaRunEdition.Default;

                        RCFRCP.File.MoveDirectory(fiestaBackupDir, Games.RaymanFiestaRun.GetBackupDir(), true);
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
                    RCFRCP.File.DeleteDirectory(Path.Combine(Path.GetTempPath(), "RCP_Metro"));
                }
                catch (Exception ex)
                {
                    ex.HandleError("Cleaning pre-5.0.0 temp");
                }

                Data.DisableDowngradeWarning = false;
            }

            if (Data.LastVersion < new Version(5, 1, 0, 0))
                Data.EducationalDosBoxGames = null;

            // Re-deploy files
            await RCFRCP.App.DeployFilesAsync(true);

            // Force refresh the Registry value to reflect the new update
            Data.PendingRegUninstallKeyRefresh = true;

            // Refresh the jump list
            RefreshJumpList();

            // Show app news
            new AppNewsDialog().ShowDialog();

            // Update the last version
            Data.LastVersion = RCFRCP.App.CurrentVersion;
        }

        #endregion

        #region Event Handlers

        private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            using (await DataChangedHandlerAsyncLock.LockAsync())
            {
                switch (e.PropertyName)
                {
                    case nameof(AppUserData.DarkMode):
                        ThemeManager.ChangeAppTheme(Application.Current, $"Base{(Data.DarkMode ? "Dark" : "Light")}");
                        break;

                    case nameof(AppUserData.BackupLocation):

                        await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, true, false));

                        if (!PreviousBackupLocation.DirectoryExists)
                        {
                            RCFCore.Logger?.LogInformationSource("The backup location has been changed, but the previous directory does not exist");
                            return;
                        }

                        RCFCore.Logger?.LogInformationSource("The backup location has been changed and old backups are being moved...");

                        await RCFRCP.App.MoveBackupsAsync(PreviousBackupLocation, Data.BackupLocation);

                        PreviousBackupLocation = Data.BackupLocation;

                        break;

                    case nameof(AppUserData.ShowIncompleteTranslations):
                        AppLanguages.RefreshLanguages(Data.ShowIncompleteTranslations);
                        break;

                    case nameof(AppUserData.LinkItemStyle):

                        string GetStyleSource(LinkItemStyles linkItemStye)
                        {
                            return $"{AppViewModel.ApplicationBasePath}/Styles/LinkItemStyles - {linkItemStye}.xaml";
                        }

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

                    case nameof(AppUserData.FiestaRunVersion):

                        // Update the install directory and game info
                        try
                        {
                            GameInfo GameInfo = new GameInfo(GameType.WinStore, Games.RaymanFiestaRun.GetGameManager<WinStoreGameManager>().GetPackageInstallDirectory());
                            RCFRCP.Data.Games[Games.RaymanFiestaRun] = GameInfo;
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Getting updated Windows Store game install directory");
                        }

                        await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanFiestaRun, false, false, true, true));

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
                    // Create a jump list
                    new JumpList(RCFRCP.App.GetGames.
                            // Add only games which have been added
                            Where(x => x.IsAdded()).
                            // Create a jump task item for each game
                            Select(x =>
                            {
                                var manager = x.GetGameManager();
                                var launchInfo = manager.GetLaunchInfo();

                                return new JumpTask()
                                {
                                    Title = x.GetDisplayName(),
                                    Description = String.Format(Metro.Resources.JumpListItemDescription, x.GetDisplayName()),
                                    ApplicationPath = launchInfo.Path,
                                    WorkingDirectory = launchInfo.Path.FileExists ? launchInfo.Path.Parent : FileSystemPath.EmptyPath,
                                    Arguments = launchInfo.Args,
                                    IconResourcePath = manager.GetIconResourcePath()
                                };
                            }), false, false).
                        // Apply the new jump list
                        Apply();
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
        private LinkItemStyles PreviousLinkItemStyle { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion
    }
}