using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using RayCarrot.CarrotFramework;
using RayCarrot.Rayman;
using RayCarrot.RCP.Metro.Legacy;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public App()
        {
            // Use mutex to only allow one instance of the application at a time
            Mutex = new Mutex(false, "Global\\" + ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value);
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The mutex
        /// </summary>
        private Mutex Mutex { get; }

        #endregion

        #region Event Handlers

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                if (!Mutex.WaitOne(0, false))
                {
                    MessageBox.Show("An instance of the Rayman Control Panel is already running", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }
            catch (AbandonedMutexException)
            {
                // Break if debugging
                Debugger.Break();
            }

            // Make sure we are on Windows Vista or higher for the Windows API Code Pack
            if (!CommonFileDialog.IsPlatformSupported)
            {
                MessageBox.Show("Windows Vista or higher is required to run this application", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AppStartupAsync(e.Args);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                if (RCF.IsBuilt)
                {
                    // Handle the exception
                    e.Exception.HandleCritical("Unhandled exception");

                    RCF.Logger.LogCriticalSource("An unhandled exception has occurred");
                }

                // Get the path to log to
                FileSystemPath logPath = Path.Combine(Directory.GetCurrentDirectory(), "crashlog.txt");

                // Write log
                File.WriteAllLines(logPath, SessionLogger.Logs?.Select(x => $"[{x.LogLevel}] {x.Message}") ?? new string[] { "Service not available" });

                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
                                $"A crash log has been created under {logPath}.", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
                                $"The log can be found under {CommonPaths.LogFile}.", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Dispose mutex
                Mutex?.Dispose();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Dispose mutex
            Mutex?.Dispose();
        }

        private void App_RefreshRequired(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
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
                                Description = $"Launch {x.GetDisplayName()}",
                                ApplicationPath = launchInfo.Path,
                                Arguments = launchInfo.Args,
                                CustomCategory = "Game Shortcuts",
                                IconResourcePath = info.GameType == GameType.DosBox || info.GameType == GameType.Steam ? info.InstallDirectory + x.GetLaunchName() : launchInfo.Path
                            };
                        }), false, false).
                    // Apply the new jump list
                    Apply();
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the application startup
        /// </summary>
        /// <param name="args">The launch arguments</param>
        private async void AppStartupAsync(string[] args)
        {
            // Set the shutdown mode to avoid the license window to close the application
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Make sure the license is accepted
            if (!ShowLicense())
            {
                Shutdown();
                return;
            }

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

            // Set up the framework
            await SetupFrameworkAsync(args);

            // Subscribe to when games need to be refreshed
            RCFRCP.App.RefreshRequired += App_RefreshRequired;

            // Log the current environment
            LogEnvironment();

            // Log some debug information
            RCF.Logger.LogDebugSource($"Executing assembly path: {Assembly.GetExecutingAssembly().Location}");

            // Listen to data binding logs
            PresentationTraceSources.DataBindingSource.Listeners.Add(new RCPTraceListener());

            // Run basic startup
            await BasicStartupAsync();

            // Run post-update code
            await PostUpdateAsync();

            // Clean temp folder
            RCFRCP.File.DeleteDirectory(CommonPaths.TempPath);

            // Create the temp folder
            Directory.CreateDirectory(CommonPaths.TempPath);

            RCF.Logger.LogInformationSource($"The temp directory has been created");

            // Set the startup URI
            MainWindow = new MainWindow();
            MainWindow.Show();

            // Set the shutdown mode
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        /// <summary>
        /// Sets up the framework
        /// </summary>
        private static async Task SetupFrameworkAsync(string[] args)
        {
            LogLevel logLevel = LogLevel.Information;

            // Get the log level from launch arguments
            if (args.Contains("-loglevel"))
            {
                try
                {
                    string ll = args[args.FindItemIndex(x => x == "-loglevel") + 1];
                    logLevel = Enum.Parse(typeof(LogLevel), ll, true).CastTo<LogLevel>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Setting user level from args");
                }
            }

            new FrameworkConstruction().
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
                // Add app handler
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
                AddTransient<BackupManager>().
                // Build the framework
                Build();

            RCF.Logger.LogInformationSource($"The log level has been set to {logLevel}");

            // Retrieve arguments
            RCF.Data.Arguments = args;

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

            if (RCFRCP.Data.IsFirstLaunch)
                await ImportLegacyDataAsync();
        }

        /// <summary>
        /// Logs information about the current environment
        /// </summary>
        private static void LogEnvironment()
        {
            try
            {
                RCF.Logger.LogTraceSource($"Current Platform: {Environment.OSVersion.VersionString}");

            }
            catch (Exception ex)
            {
                ex.HandleError("Logging environment details");
            }
        }

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
        private static async Task PostUpdateAsync()
        {
            RCF.Logger.LogInformationSource($"Current version is {RCFRCP.App.CurrentVersion}");

            // Make sure this is a new version
            if (!(RCFRCP.Data.LastVersion < RCFRCP.App.CurrentVersion))
            {
                // Check if it's a lower version than previously recorded
                if (RCFRCP.Data.LastVersion > RCFRCP.App.CurrentVersion)
                {
                    RCF.Logger.LogWarningSource($"A newer version $({RCFRCP.Data.LastVersion}) has been recorded in the application data");

                    await RCF.MessageUI.DisplayMessageAsync($"You are using an older version of the program {RCFRCP.App.CurrentVersion} compared to the version of the current app data {RCFRCP.Data.LastVersion}. " +
                                                            $"This is not recommended and may cause compatibility issues. These may be fixed by resetting the app data for this program.", "Downgrade detected", MessageType.Warning);
                }

                return;
            }

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
                data.AutoLocateGames = appData.AutoGameCheck;
                data.AutoUpdate = appData.AutoUpdateCheck;  
                data.CloseAppOnGameLaunch = appData.AutoClose;
                data.CloseConfigOnSave = appData.AutoCloseConfig;
                data.ShowActionComplete = appData.ShowActionComplete;
                data.ShowProgressOnTaskBar = appData.ShowTaskBarProgress;
                data.UserLevel = appData.UserLevel;
                data.DisplayExceptionLevel = appData.DisplayExceptionLevel;

                if (appData.BackupLocation.DirectoryExists)
                    data.BackupLocation = appData.BackupLocation;

                // Import game data properties
                if (gameData.DosBoxConfig.FileExists)
                    data.DosBoxConfig = gameData.DosBoxConfig;
                if (gameData.DosBoxExe.FileExists)
                    data.DosBoxPath = gameData.DosBoxExe;

                var TPLSDir = gameData.TPLSDir + "TPLS";

                if (gameData.TPLSIsInstalled && TPLSDir.DirectoryExists)
                    data.TPLSData = new TPLSData(TPLSDir)
                    {
                        RaymanVersion = gameData.TPLSRaymanVersion.GetCurrent() ?? TPLSRaymanVersion.Auto,
                        DosBoxVersion = gameData.TPLSDOSBoxVersion.GetCurrent() ?? TPLSDOSBoxVersion.DOSBox_0_74
                    };
                
                // Import games
                foreach (LegacyRaymanGame game in gameData.RayGames)
                {
                    // Convert legacy values
                    var currentGame = game.Game.GetCurrent();
                    var currentType = game.Type.GetCurrent();

                    // Make sure we got current valid values
                    if (currentGame == null || currentType == null)
                        continue;

                    // Ignore Windows store games
                    if (currentType == GameType.WinStore)
                        continue;

                    // Make sure the game is valid
                    if (!currentGame.Value.IsValid(currentType.Value, game.Dir))
                        continue;

                    // Add the game
                    await RCFRCP.App.AddNewGameAsync(currentGame.Value, currentType.Value, game.Dir.DirectoryExists ? game.Dir : null);

                    // Add mount directory if DosBox game
                    if (currentType == GameType.DosBox)
                        RCFRCP.Data.DosBoxGames[currentGame.Value].MountPath = game.MountDir;
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
    }
}