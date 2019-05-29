using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;
using RayCarrot.RCP.Metro.Legacy;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using JumpList = System.Windows.Shell.JumpList;

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
        public App() : base(true) { }

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
        protected override void SetupFramework(FrameworkConstruction construction, LogLevel logLevel)
        {
            construction.
                // Add console, debug, session and file loggers
                AddLoggers(DefaultLoggers.Console | DefaultLoggers.Debug | DefaultLoggers.Session, logLevel, builder => builder.AddProvider(new BaseLogProvider<FileLogger>())).
                // Add a serializer
                AddSerializer(DefaultSerializers.Json).
                // Add exception handler
                AddExceptionHandler<RCPExceptionHandler>().
                // Add user data manager
                AddUserDataManager().
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
                // Add game manager
                AddTransient<GameManager>().
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
            // Load the user data
            try
            {
                await RCFData.UserDataCollection.AddUserDataAsync<AppUserData>(CommonPaths.AppUserDataPath);
                RCF.Logger.LogInformationSource($"The app user data has been loaded");
            }
            catch (Exception ex)
            {
                ex.HandleError($"Loading app user data");
                MessageBox.Show($"An error occurred reading saved app data. Some settings may be reset to their default values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RCFData.UserDataCollection.Add(new AppUserData());
                RCFRCP.Data.Reset();
            }

            // Apply the current culture if defaulted
            if (RCFRCP.Data.CurrentCulture == AppLanguages.DefaultCulture.Name)
                RCFRCP.Data.RefreshCulture(AppLanguages.DefaultCulture.Name);

            // Attempt to import legacy data on first launch
            if (RCFRCP.Data.IsFirstLaunch)
                await ImportLegacyDataAsync();

            // Subscribe to when to refresh the jump list
            RCFRCP.App.RefreshRequired += (s, e) => RefreshJumpList();
            RCF.Data.CultureChanged += (s, e) => RefreshJumpList();

            // Listen to data binding logs
            WPFTraceListener.Setup(LogLevel.Warning);

            // Run basic startup
            await BasicStartupAsync();

            // Run post-update code
            await PostUpdateAsync();

            // Clean temp folder
            RCFRCP.File.DeleteDirectory(CommonPaths.TempPath);

            // Create the temp folder
            Directory.CreateDirectory(CommonPaths.TempPath);

            RCF.Logger.LogInformationSource($"The temp directory has been created");
        }

        /// <summary>
        /// An optional method to override which runs when closing
        /// </summary>
        /// <param name="mainWindow">The main Window of the application</param>
        /// <returns>The task</returns>
        protected override async Task OnCloseAsync(Window mainWindow)
        {
            // Save state
            RCFRCP.Data.WindowState = WindowSessionState.GetWindowState(mainWindow);

            RCF.Logger.LogInformationSource($"The application is exiting...");

            // Clean the temp
            RCFRCP.App.CleanTemp();

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
        private static async Task BasicStartupAsync()
        {
            // Check for reset argument
            if (RCF.Data.Arguments.Contains("-reset"))
                RCFRCP.Data.Reset();

            // Check for user level argument
            if (RCF.Data.Arguments.Contains("-ul"))
            {
                try
                {
                    string ul = RCF.Data.Arguments[RCF.Data.Arguments.FindItemIndex(x => x == "-ul") + 1];
                    RCFRCP.Data.UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Setting user level from args");
                }
            }

            // Check for updater install argument
            if (RCF.Data.Arguments.Contains("-install"))
            {
                try
                {
                    FileSystemPath updateFile = RCF.Data.Arguments[RCF.Data.Arguments.FindItemIndex(x => x == "-install") + 1];
                    if (updateFile.FileExists)
                    {
                        updateFile.GetFileInfo().Delete();
                        RCF.Logger.LogInformationSource($"The updater was deleted");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Deleting updater");
                }
            }

            // Show first launch info
            if (RCFRCP.Data.IsFirstLaunch)
            {
                new FirstLaunchInfoDialog().ShowDialog();
                RCFRCP.Data.IsFirstLaunch = false;
            }

            await RCFRCP.App.EnableUbiIniWriteAccessAsync();
        }

        /// <summary>
        /// Runs the post-update code
        /// </summary>
        private async Task PostUpdateAsync()
        {
            RCF.Logger.LogInformationSource($"Current version is {RCFRCP.App.CurrentVersion}");

            // Make sure this is a new version
            if (!(RCFRCP.Data.LastVersion < RCFRCP.App.CurrentVersion))
            {
                // Check if it's a lower version than previously recorded
                if (RCFRCP.Data.LastVersion > RCFRCP.App.CurrentVersion)
                {
                    RCF.Logger.LogWarningSource($"A newer version $({RCFRCP.Data.LastVersion}) has been recorded in the application data");

                    await RCF.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.DowngradeWarning, RCFRCP.App.CurrentVersion, RCFRCP.Data.LastVersion), Metro.Resources.DowngradeWarningHeader, MessageType.Warning);
                }

                return;
            }

            if (RCFRCP.Data.LastVersion < new Version(4, 0, 0, 6))
                RCFRCP.Data.EnableAnimations = true;

            if (RCFRCP.Data.LastVersion < new Version(4, 1, 1, 0))
                RCFRCP.Data.ShowIncompleteTranslations = false;

            if (RCFRCP.Data.LastVersion < new Version(4, 5, 0, 0))
                RCFRCP.Data.LinkItemStyle = LinkItemStyles.List;

            // Refresh the jump list
            RefreshJumpList();

            // Show app news
            new AppNewsDialog().ShowDialog();

            // Update the last version
            RCFRCP.Data.LastVersion = RCFRCP.App.CurrentVersion;
        }

        /// <summary>
        /// Imports legacy app data from version 2.x to 3.x
        /// </summary>
        /// <returns>The task</returns>
        private static async Task ImportLegacyDataAsync()
        {
            try
            {
                // Get the legacy file locations
                FileSystemPath baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rayman Control Panel");
                var appDataLocation = baseDir + "appuserdata.json";
                var gameDataLocation = baseDir + "gameuserdata.json";

                // Make sure the files exist
                if (!appDataLocation.FileExists || !gameDataLocation.FileExists)
                    return;

                // Create the serializer
                var s = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });

                // Load the data
                LegacyAppUserData appData = new StringReader(File.ReadAllText(appDataLocation)).RunAndDispose(x =>
                    new JsonTextReader(x).RunAndDispose(y => s.Deserialize<LegacyAppUserData>(y)));
                LegacyGameUserData gameData = new StringReader(File.ReadAllText(gameDataLocation)).RunAndDispose(x =>
                    new JsonTextReader(x).RunAndDispose(y => s.Deserialize<LegacyGameUserData>(y)));

                RCF.Logger.LogInformationSource($"Legacy app data found from version {appData.LastVersion}");

                // Ask the user
                //if (!await RCF.MessageUI.DisplayMessageAsync($"Application data was found for version {appData.LastVersion}. Do you want to import it?", "Import data", MessageType.Question, true))
                //    return;

                // Get current app data
                var data = RCFRCP.Data;

                // Import app data properties
                data.AutoLocateGames = appData.AutoGameCheck ?? true;
                data.AutoUpdate = appData.AutoUpdateCheck ?? true;
                data.CloseAppOnGameLaunch = appData.AutoClose ?? false;
                data.CloseConfigOnSave = appData.AutoCloseConfig ?? true;
                data.ShowActionComplete = appData.ShowActionComplete ?? true;
                data.ShowProgressOnTaskBar = appData.ShowTaskBarProgress ?? true;
                data.UserLevel = appData.UserLevel ?? UserLevel.Normal;
                data.DisplayExceptionLevel = appData.DisplayExceptionLevel ?? ExceptionLevel.Critical;

                if (appData.BackupLocation?.DirectoryExists == true)
                    data.BackupLocation = appData.BackupLocation.Value;

                // Import game data properties
                if (gameData.DosBoxConfig?.FileExists == true)
                    data.DosBoxConfig = gameData.DosBoxConfig.Value;
                if (gameData.DosBoxExe?.FileExists == true)
                    data.DosBoxPath = gameData.DosBoxExe.Value;

                var TPLSDir = gameData.TPLSDir != null ? gameData.TPLSDir.Value + "TPLS" : FileSystemPath.EmptyPath;

                if (gameData.TPLSIsInstalled == true && TPLSDir.DirectoryExists)
                    data.TPLSData = new TPLSData(TPLSDir)
                    {
                        RaymanVersion = gameData.TPLSRaymanVersion?.GetCurrent() ?? TPLSRaymanVersion.Auto,
                        DosBoxVersion = gameData.TPLSDOSBoxVersion?.GetCurrent() ?? TPLSDOSBoxVersion.DOSBox_0_74
                    };
                
                if (gameData.RayGames != null)
                {
                    // Import games
                    foreach (LegacyRaymanGame game in gameData.RayGames)
                    {
                        // Convert legacy values
                        var currentGame = game.Game?.GetCurrent();
                        var currentType = game.Type?.GetCurrent();

                        // Make sure we got current valid values
                        if (currentGame == null || currentType == null)
                            continue;

                        // Ignore Windows store games
                        if (currentType == GameType.WinStore)
                            continue;

                        // Make sure the game is valid
                        if (!currentGame.Value.IsValid(currentType.Value, game.Dir ?? FileSystemPath.EmptyPath))
                            continue;

                        // Add the game
                        await RCFRCP.App.AddNewGameAsync(currentGame.Value, currentType.Value, game.Dir?.DirectoryExists == true ? game.Dir : null);

                        // Add mount directory if DosBox game
                        if (currentType == GameType.DosBox && game.MountDir != null)
                            RCFRCP.Data.DosBoxGames[currentGame.Value].MountPath = game.MountDir.Value;
                    }
                }

                // Remove old data
                RCFRCP.File.DeleteFile(appDataLocation);
                RCFRCP.File.DeleteFile(gameDataLocation);
            }
            catch (Exception ex)
            {
                ex.HandleError("Import legacy data");
                await RCF.MessageUI.DisplayMessageAsync("An error occurred when importing legacy data", MessageType.Error);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the application jump list
        /// </summary>
        public void RefreshJumpList()
        {
            Dispatcher.Invoke(() =>
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
                                var launchInfo = x.GetLaunchInfo();
                                var info = x.GetInfo();

                                return new JumpTask()
                                {
                                    Title = x.GetDisplayName(),
                                    Description = String.Format(Metro.Resources.JumpListItemDescription, x.GetDisplayName()),
                                    ApplicationPath = launchInfo.Path,
                                    WorkingDirectory = info.GameType == GameType.Win32 || info.GameType == GameType.DosBox ? launchInfo.Path.Parent : FileSystemPath.EmptyPath,
                                    Arguments = launchInfo.Args,
                                    IconResourcePath = info.GameType == GameType.DosBox || info.GameType == GameType.Steam ? info.InstallDirectory + x.GetLaunchName() : launchInfo.Path,
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

        #region Public Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion
    }
}