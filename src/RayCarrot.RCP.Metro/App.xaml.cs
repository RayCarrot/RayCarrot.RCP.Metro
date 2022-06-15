using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace RayCarrot.RCP.Metro;

// TODO: Move code out of here to StartupManager. This should only set up WPF/XAML related stuff.

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    #region Constructor

    public App(string[] args)
    {
        // Set up the services
        IServiceCollection services = new ServiceCollection();
        ConfigureServices(services, args);
        ServiceProvider = services.BuildServiceProvider();

        // Create properties
        DataChangedHandlerAsyncLock = new AsyncLock();
        StartupEventsCalledAsyncLock = new AsyncLock();
        HasRunStartupEvents = false;

        // Set services
        AppVM = ServiceProvider.GetRequiredService<AppViewModel>();
        Data = ServiceProvider.GetRequiredService<AppUserData>();

#if DEBUG
        // Create the startup timer and start it
        AppStartupTimer = new Stopwatch();
        AppStartupTimer.Start();

        StartupTimeLogs = new List<string>();
#endif

        LogStartupTime("App: Showing splash screen");

        // Create and show the splash screen
        SplashScreenFadeoutTime = TimeSpan.FromMilliseconds(200);
        SplashScreen = new SplashScreen(SplashScreenResourceName);
        SplashScreen.Show(false);

        // Subscribe to events
        Startup += App_Startup;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        Exit += App_Exit;

        LogStartupTime("App: Checking Mutex");

        try
        {
            Assembly? entry = Assembly.GetEntryAssembly();

            if (entry is null)
                throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if no valid entry assembly is found");

            // Use mutex to only allow one instance of the application at a time
            Mutex = new Mutex(false, "Global\\" + ((GuidAttribute)entry.GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value);
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if the entry assembly does not have a valid GUID identifier", ex);
        }

        LogStartupTime("BaseApp: Construction finished");
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constant Fields

    private const string SplashScreenResourceName = "Files/Splash Screen.png";

    #endregion

    #region Services

    private AppViewModel AppVM { get; }
    private AppUserData Data { get; }

    #endregion

    #region Private Properties

    /// <summary>
    /// The async lock for <see cref="Data_PropertyChangedAsync"/>
    /// </summary>
    private AsyncLock DataChangedHandlerAsyncLock { get; }

    /// <summary>
    /// Async lock for calling the startup events
    /// </summary>
    private AsyncLock StartupEventsCalledAsyncLock { get; }

    /// <summary>
    /// The splash screen, if one is used
    /// </summary>
    private SplashScreen? SplashScreen { get; }

    /// <summary>
    /// The mutex
    /// </summary>m
    private Mutex? Mutex { get; }

    /// <summary>
    /// The timer for the application startup
    /// </summary>
    private Stopwatch? AppStartupTimer { get; }

    /// <summary>
    /// The startup time logs to log once the app has started to improve performance
    /// </summary>
    private List<string>? StartupTimeLogs { get; }

    /// <summary>
    /// The fadeout for the splash screen
    /// </summary>
    private TimeSpan SplashScreenFadeoutTime { get; }

    /// <summary>
    /// Indicates if the startup events have run
    /// </summary>
    private bool HasRunStartupEvents { get; set; }

    /// <summary>
    /// Indicates if the main window is currently closing
    /// </summary>
    private bool IsClosing { get; set; }

    /// <summary>
    /// Indicates if the main window is done closing and can be closed
    /// </summary>
    private bool DoneClosing { get; set; }

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

    public new static App Current => Application.Current as App ?? throw new InvalidOperationException($"Current app is not a valid {nameof(App)}");

    /// <summary>
    /// The service provider for this application
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    public bool IsLogViewerAvailable => LogViewerViewModel != null;
    public LogViewerViewModel? LogViewerViewModel { get; set; }

    public MainWindow? ChildWindowsParent => MainWindow as MainWindow;

    #endregion

    #region Event Handlers

    private async void App_Startup(object sender, StartupEventArgs e)
    {
        bool result = await AppStartupAsync(e.Args);

        if (!result)
            Shutdown();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            // Log the exception
            Logger.Fatal(e.Exception, "Unhandled exception");

            // Get the path to log to
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "crashlog.txt");

            // Write log
            File.WriteAllLines(logPath, LogManager.Configuration.FindTargetByName<MemoryTarget>("memory")?.Logs ?? new string[]
            {
                "Service not available",
                Environment.NewLine,
                e.Exception?.ToString() ?? "<No Exception>"
            });

            // Notify user
            MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e.Exception?.Message}" +
                            $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}A crash log has been created under {logPath}.",
                "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception)
        {
            // Notify user
            MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e.Exception?.Message}",
                "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Close splash screen
            CloseSplashScreen();

            // Dispose
            Dispose();

            // Close the logger
            LogManager.Shutdown();

            if (MainWindow is MainWindow m)
                m.ViewModel.Dispose();
        }
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        // Close splash screen
        CloseSplashScreen();

        // Dispose
        Dispose();
    }

    private async void MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
    {
        // Add startup time log
        LogStartupTime("MainWindow: Main window loaded");

#if DEBUG
        // Stop the stopwatch
        AppStartupTimer?.Stop();

        // Log all startup time logs
        foreach (string log in StartupTimeLogs!)
            Logger.Debug(log);

        // Clear the startup time logs
        StartupTimeLogs.Clear();
#endif

        using (await StartupEventsCalledAsyncLock.LockAsync())
        {
            // Call all startup events
            await (LocalStartupComplete?.RaiseAllAsync(this, EventArgs.Empty) ?? Task.CompletedTask);

            // Remove events as they'll not get called again
            LocalStartupComplete = null;

            AppVM.IsStartupRunning = false;

            HasRunStartupEvents = true;
        }
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        // Shutdown the application
        Shutdown();
    }

    private async void MainWindow_ClosingAsync(object sender, CancelEventArgs e)
    {
        // Ignore if already closed
        if (DoneClosing)
            return;

        // Cancel the native closing
        e.Cancel = true;

        // Don't close if the close button is disabled
        if (sender is MetroWindow { IsCloseButtonEnabled: false })
            return;

        // If already is closing, ignore
        if (IsClosing)
            return;

        Logger.Info("The main window is closing...");

        // Shut down the app
        await ShutdownAppAsync(false);
    }

    private async Task App_StartupComplete_Updater_Async(object sender, EventArgs eventArgs)
    {
        // Clean up deployed files
        await ServiceProvider.GetRequiredService<DeployableFilesManager>().CleanupFilesAsync();

        // Check for updates
        if (Data.Update_AutoUpdate)
            await AppVM.CheckForUpdatesAsync(false);
    }

    private Task App_StartupComplete_Miscellaneous_Async(object sender, EventArgs eventArgs)
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

        // Run on UI thread
        Dispatcher.Invoke(SecretCodeManager.Setup);

        return Task.CompletedTask;
    }

    private async Task App_StartupComplete_GameFinder_Async(object sender, EventArgs eventArgs)
    {
        // Check for installed games
        if (Data.Game_AutoLocateGames)
            await AppVM.RunGameFinderAsync();
    }

    private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
    {
        // TODO: Check this to make sure it won't accidentally deadlock the app. What if something in here updates the data?
        using (await DataChangedHandlerAsyncLock.LockAsync())
        {
            switch (e.PropertyName)
            {
                case nameof(AppUserData.Theme_DarkMode):
                case nameof(AppUserData.Theme_SyncTheme):
                    this.SetTheme(Data.Theme_DarkMode, Data.Theme_SyncTheme);
                    break;

                case nameof(AppUserData.Backup_BackupLocation):

                    await AppVM.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.Backups));

                    if (!PreviousBackupLocation.DirectoryExists)
                    {
                        Logger.Info("The backup location has been changed, but the previous directory does not exist");
                        return;
                    }

                    Logger.Info("The backup location has been changed and old backups are being moved...");

                    await AppVM.MoveBackupsAsync(PreviousBackupLocation, Data.Backup_BackupLocation);

                    PreviousBackupLocation = Data.Backup_BackupLocation;

                    break;

                case nameof(AppUserData.UI_LinkItemStyle):
                    static string GetStyleSource(UserData_LinkItemStyle linkItemStye) => $"{AppViewModel.WPFApplicationBasePath}/UI/Resources/Styles.LinkItem.{linkItemStye}.xaml";

                    // Get previous source
                    string oldSource = GetStyleSource(PreviousLinkItemStyle);

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
                    await AppVM.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanRavingRabbids2, RefreshFlags.GameInfo | RefreshFlags.LaunchInfo));
                    break;

                case nameof(AppUserData.Emu_DOSBox_Path):
                case nameof(AppUserData.Emu_DOSBox_ConfigPath):
                    await AppVM.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.GameInfo));
                    break;
            }
        }
    }

    #endregion

    #region Private Methods

    private void ConfigureServices(IServiceCollection serviceCollection, string[] args)
    {
        // Add app related services
        serviceCollection.AddTransient<LicenseManager>();
        serviceCollection.AddTransient<GamesManager>();
        serviceCollection.AddSingleton<AppViewModel>();
        serviceCollection.AddSingleton<AppUserData>();
        serviceCollection.AddSingleton<IAppInstanceData>(_ => new AppInstanceData()
        {
            Arguments = args
        });
        serviceCollection.AddTransient<IFileManager, RCPFileManager>();
        serviceCollection.AddTransient<IUpdaterManager, RCPUpdaterManager>();
        serviceCollection.AddTransient<GameBackups_Manager>();
        serviceCollection.AddSingleton<DeployableFilesManager>();

        // Add the main window
        serviceCollection.AddSingleton<MainWindow>();
        serviceCollection.AddSingleton<MainWindowViewModel>();

        // Add the pages
        serviceCollection.AddSingleton<Page_Games_ViewModel>();
        serviceCollection.AddSingleton<Page_Progression_ViewModel>();
        serviceCollection.AddSingleton<Page_Utilities_ViewModel>();
        serviceCollection.AddSingleton<Page_Mods_ViewModel>();
        serviceCollection.AddSingleton<Page_Settings_ViewModel>();
        serviceCollection.AddSingleton<Page_About_ViewModel>();
        serviceCollection.AddSingleton<Page_Debug_ViewModel>();

        // Add UI managers
        serviceCollection.AddSingleton<IDialogBaseManager, RCPWindowDialogBaseManager>();
        serviceCollection.AddTransient<IMessageUIManager, RCPMessageUIManager>();
        serviceCollection.AddTransient<IBrowseUIManager, RCPBrowseUIManager>();
        serviceCollection.AddTransient<AppUIManager>();
    }

    /// <summary>
    /// Handles the application startup
    /// </summary>
    /// <param name="args">The launch arguments</param>
    private async Task<bool> AppStartupAsync(string[] args)
    {
        LogStartupTime("Startup: App startup begins");

        try
        {
            if (Mutex is not null && !Mutex.WaitOne(0, false))
            {
                MessageBox.Show($"An instance of the Rayman Control Panel is already running", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
#pragma warning disable 168
        catch (AbandonedMutexException _)
#pragma warning restore 168
        {
            // Break if debugging
            Debugger.Break();
        }

        // Set the shutdown mode to avoid any license windows closing the application
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        LogStartupTime("Startup: Checking Windows version");

        // Make sure we are on Windows Vista or higher for APIs such as the Windows API Code Pack and Deployment Image Servicing and Management
        if (AppViewModel.WindowsVersion < WindowsVersion.WinVista && AppViewModel.WindowsVersion != WindowsVersion.Unknown)
        {
            MessageBox.Show("Windows Vista or higher is required to run this application", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        LogStartupTime("Startup: Checking license");

        // Make sure the license has been accepted
        if (!VerifyLicense(ServiceProvider.GetRequiredService<LicenseManager>()))
            return false;

        LogStartupTime("Startup: Setting default directory");

        // Hard code the current directory to avoid any issues with the application launching from other locations than its own
        string? assemblyPath = Assembly.GetEntryAssembly()?.Location;
        string? assemblyDir = Path.GetDirectoryName(assemblyPath);
            
        if (assemblyDir != null)
            Directory.SetCurrentDirectory(assemblyDir);

        LogStartupTime("Startup: Initial setup has been verified");

        // Initialize the logging
        InitializeLogging(args);

        // Log the current environment
        try
        {
            Logger.Info("Current platform: {0}", Environment.OSVersion.VersionString);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Logging environment details");
        }

        // Log some debug information
        Logger.Debug("Entry assembly path: {0}", assemblyPath);

        LogStartupTime("Startup: Debug info has been logged");

        // Run RCP startup
        await SetupRCPAsync(args, assemblyPath);

        LogStartupTime("Startup: Creating main window");

        // Create the main window
        MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

        // Load previous state
        Data.UI_WindowState?.ApplyToWindow(mainWindow);

        LogStartupTime("Startup: Main window has been created");

        // Subscribe to window events
        mainWindow.Loaded += MainWindow_LoadedAsync;
        mainWindow.Closing += MainWindow_ClosingAsync;
        mainWindow.Closed += MainWindow_Closed;

        // Close splash screen
        CloseSplashScreen();

        // Show the main window
        mainWindow.Show();

        // Set the shutdown mode
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        return true;
    }

    /// <summary>
    /// Shows the application license message and returns a value indicating if it was accepted
    /// </summary>
    /// <returns>True if it was accepted, false if not</returns>
    private bool VerifyLicense(LicenseManager license)
    {
        try
        {
            if (license.HasAcceptedLicense())
                return true;

            // Close the splash screen
            CloseSplashScreen();

            // Show the license prompt
            return license.PrompLicense();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The license verification failed with the message of: {ex.Message}", "License error", MessageBoxButton.OK, MessageBoxImage.Error);

            return false;
        }
    }

    private void InitializeLogging(IList<string> args)
    {
        // Create a new logging configuration
        LoggingConfiguration logConfig = new();

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

        const string logLayout = "${time:invariant=true}|${level:uppercase=true}|${logger}|${message:withexception=true}";
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
                // (starting with NLog 5.0 these are the default values, but let's be explicit anyway)
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
                    LogItemViewModel log = new(logEvent.Level, logEvent.Exception, logEvent.TimeStamp, logEvent.LoggerName, logEvent.FormattedMessage);
                    log.IsVisible = log.LogLevel >= LogViewerViewModel.ShowLogLevel;
                    LogViewerViewModel.LogItems.Add(log);
                });
            }));
        }

        // Apply config
        LogManager.Configuration = logConfig;
    }

    private async Task SetupRCPAsync(IList<string> args, string? assemblyPath)
    {
        LogStartupTime("Setup: RCP setup is starting");

        // Load the user data
        try
        {
            // Read the data from the file if it exists
            if (!Services.InstanceData.Arguments.Contains("-reset") && AppFilePaths.AppUserDataPath.FileExists)
            {
                // Always reset the data first so any missing properties use the correct defaults
                Data.Reset();

                Data.App_LastVersion = null; // Need to set to null before calling JsonConvert.PopulateObject or else it's ignored

                // Populate the data from the file
                JsonConvert.PopulateObject(File.ReadAllText(AppFilePaths.AppUserDataPath), Data);

                Logger.Info("The app user data has been loaded");

                // Verify the data
                Data.Verify();
            }
            else
            {
                // Reset the user data
                Data.Reset();

                Logger.Info("The app user data has been reset");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading app user data");

            // NOTE: This is not localized due to the current culture not having been set at this point
            MessageBox.Show("An error occurred reading saved app data. The settings have been reset to their default values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Reset the user data
            Data.Reset();
        }

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

        LogStartupTime("Setup: Subscribing to events");

        StartupComplete += App_StartupComplete_Miscellaneous_Async;
        StartupComplete += App_StartupComplete_Updater_Async;
        StartupComplete += App_StartupComplete_GameFinder_Async;

        // Track changes to the user data
        Data.PropertyChanged += Data_PropertyChangedAsync;
        PreviousLinkItemStyle = Data.UI_LinkItemStyle;
        PreviousBackupLocation = Data.Backup_BackupLocation;

        // Subscribe to when to refresh the jump list
        AppVM.RefreshRequired += (_, e) =>
        {
            if (e.GameCollectionModified || e.GameInfoModified || e.JumpListModified)
                RefreshJumpList();

            return Task.CompletedTask;
        };
        Services.InstanceData.CultureChanged += (_, _) => RefreshJumpList();

        LogStartupTime("Setup: Checking launch arguments");

        CheckLaunchArguments(args);

        // Update the application path
        if (assemblyPath != Data.App_ApplicationPath)
        {
            Data.App_ApplicationPath = assemblyPath;
            Logger.Info("The application path has been updated");
        }

        LogStartupTime("Setup: Initializing web protocol");

        try
        {
            // On Windows 7 the default TLS version is 1.0. As of December 2021 raym.app where RCP is hosted only supports
            // TLS 1.2 and 1.3 thus causing any web requests to fail on Windows 7. Since we don't want to have to hard-code the protocol
            // to use on modern system we only do so on Windows 7, leaving it as the system default otherwise (TLS 1.2 or higher)
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8 && AppViewModel.WindowsVersion != WindowsVersion.Unknown)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Initializing web protocol");
        }

        LogStartupTime("Setup: Checking for first launch");

        // Show first launch info
        if (Data.App_IsFirstLaunch ||
            // Show if updated to version 12 due to it having been changed
            Data.App_LastVersion < new Version(12, 0, 0, 5))
        {
            // Close the splash screen
            CloseSplashScreen();

            // Show the first launch dialog
            new FirstLaunchInfoDialog().ShowDialog();

            Data.App_IsFirstLaunch = false;
        }

        LogStartupTime("Setup: Validating games");

        // Validate the added games
        await ValidateGamesAsync();

        LogStartupTime("Setup: Checking if updated to new version");

        Logger.Info("Current version is {0}", AppVM.CurrentAppVersion);

        // Check if it's a new version
        if (Data.App_LastVersion < AppVM.CurrentAppVersion)
        {
            // Run post-update code
            await PostUpdateAsync();

            LogStartupTime("Setup: Post update has run");

            // Update the last version
            Data.App_LastVersion = AppVM.CurrentAppVersion;
        }
        // Check if it's a lower version than previously recorded
        else if (Data.App_LastVersion > AppVM.CurrentAppVersion)
        {
            Logger.Warn("A newer version ({0}) has been recorded in the application data", Data.App_LastVersion);

            if (!Data.Update_DisableDowngradeWarning)
                await Services.MessageUI.DisplayMessageAsync(String.Format(Metro.Resources.DowngradeWarning, AppVM.CurrentAppVersion,
                    Data.App_LastVersion), Metro.Resources.DowngradeWarningHeader, MessageType.Warning);

            LogStartupTime("Setup: Deploying files");
        }
    }

    private void CheckLaunchArguments(IList<string> args)
    {
        // Check for user level argument
        if (args.Contains("-ul"))
        {
            try
            {
                string ul = args[args.FindItemIndex(x => x == "-ul") + 1];
                Data.App_UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting user level from args");
            }
        }

        // NOTE: Starting with the updater 3.0.0 (available from 4.5.0) this is no longer used. It must however be maintained for legacy support (i.e. updating to version 4.5.0+ using an updater below 3.0.0)
        // Check for updater install argument
        if (args.Contains("-install"))
        {
            try
            {
                FileSystemPath updateFile = args[args.FindItemIndex(x => x == "-install") + 1];
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
    }

    private async Task ValidateGamesAsync()
    {
        // Keep track of removed games
        HashSet<Games> removed = new();

        // Make sure every game is valid
        foreach (Games game in AppVM.GetGames)
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
            await Services.Games.RemoveGameAsync(game, true);

            // Add to removed games
            removed.Add(game);

            Logger.Info("The game {0} has been removed due to not being valid", game);
        }

        // Refresh if any games were removed
        if (removed.Any())
            await AppVM.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(removed, RefreshFlags.GameCollection));
    }

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
            FileSystemPath fiestaBackupDir = Data.Backup_BackupLocation + AppViewModel.BackupFamily + "Rayman Fiesta Run";

            if (fiestaBackupDir.DirectoryExists)
            {
                try
                {
                    // Read the app data file
                    JObject appData = new StringReader(File.ReadAllText(AppFilePaths.AppUserDataPath)).RunAndDispose(x =>
                        new JsonTextReader(x).RunAndDispose(y => JsonSerializer.Create().Deserialize(y))).CastTo<JObject>();

                    // Get the previous Fiesta Run version
                    bool? isWin10 = appData["IsFiestaRunWin10Edition"]?.Value<bool>();

                    if (isWin10 != null)
                    {
                        // Set the current edition
                        Data.Game_FiestaRunVersion = isWin10.Value
                            ? UserData_FiestaRunEdition.Win10
                            : UserData_FiestaRunEdition.Default;

                        Services.File.MoveDirectory(fiestaBackupDir, Data.Backup_BackupLocation + AppViewModel.BackupFamily + Games.RaymanFiestaRun.GetGameInfo().BackupName, true, true);
                    }
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
            Data.App_JumpListItemIDCollection = AppVM.GetGames.
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
            string keyPath = RegistryHelpers.CombinePaths(AppFilePaths.UninstallRegistryKey, regUninstallKeyName);

            // Check if the key exists
            if (RegistryHelpers.KeyExists(keyPath))
            {
                // Make sure the user is running as admin
                if (AppVM.IsRunningAsAdmin)
                {
                    try
                    {
                        // Open the parent key
                        using RegistryKey? parentKey = RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Default, true);

                        // Delete the sub-key
                        parentKey?.DeleteSubKey(regUninstallKeyName);

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

        if (Data.App_LastVersion < new Version(13, 0, 0, 0))
        {
            Data.Progression_SaveEditorExe = FileSystemPath.EmptyPath;
            Data.Progression_ShownEditSaveWarning = false;
            Data.Backup_GameDataSources = new Dictionary<string, ProgramDataSource>();
            Data.Binary_IsSerializationLogEnabled = false;
            Data.Mod_RRR_ToggleStates = new Dictionary<string, UserData_Mod_RRR_ToggleState>();
        }

        if (Data.App_LastVersion < new Version(13, 1, 0, 0))
            Data.Archive_CNT_SyncOnRepack = false;

        // Refresh the jump list
        RefreshJumpList();

        // Close the splash screen
        CloseSplashScreen();

        // Show app news
        await Services.DialogBaseManager.ShowWindowAsync(new AppNewsDialog());
    }

    [Conditional("DEBUG")]
    private void LogStartupTime(string logDescription) => StartupTimeLogs?.Add($"Startup: {AppStartupTimer?.ElapsedMilliseconds} ms - {logDescription}");
    private void CloseSplashScreen() => SplashScreen?.Close(SplashScreenFadeoutTime);
    private void Dispose() => Mutex?.Dispose();

    #endregion

    #region Public Methods

    /// <summary>
    /// Shuts down the application
    /// </summary>
    /// <param name="forceShutDown">Indicates if the app should be forced to shut down</param>
    /// <returns>The task</returns>
    public async Task ShutdownAppAsync(bool forceShutDown)
    {
        // If already is closing, ignore
        if (IsClosing)
            return;

        // Flag that we are closing
        IsClosing = true;

        try
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                // Don't close if loading
                if (AppVM.IsLoading)
                    return;

                // Attempt to close all windows except the main one
                foreach (Window window in Windows.Cast<Window>().ToArray())
                {
                    // Ignore the main window for now
                    if (window == MainWindow)
                        continue;

                    // Focus the window
                    window.Focus();

                    // Attempt to close the window
                    window.Close();
                }

                // Attempt to close all child windows, starting with the modal ones
                foreach (RCPChildWindow childWindow in ChildWindowInstance.OpenChildWindows.OrderBy(x => x.IsModal ? 0 : 1).ToArray())
                {
                    if (childWindow is { IsMinimized: true } c)
                        c.ToggleMinimized();

                    childWindow.Close();
                }

                // Yield so that the child windows fully close before we do the next check
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                // Make sure all other windows have been closed unless forcing a shut down
                if (!forceShutDown && (Windows.Count > 1 || ChildWindowInstance.OpenChildWindows.Any()))
                {
                    Logger.Info("The shutdown was canceled due to one or more windows still being open");

                    IsClosing = false;
                    return;
                }

                // Save window state
                if (MainWindow != null)
                    Data.UI_WindowState = UserData_WindowSessionState.GetWindowState(MainWindow);

                Logger.Info("The application is exiting...");

                // Dispose
                if (MainWindow is MainWindow m)
                    m.ViewModel.Dispose();

                // Save all user data
                await AppVM.SaveUserDataAsync();

                // Close the logger
                LogManager.Shutdown();

                // Flag that we are done closing the main window
                DoneClosing = true;

                // Shut down application
                Shutdown();
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                // Attempt to log the exception, ignoring any exceptions thrown
                new Action(() => Logger.Error(ex, "Closing main window")).IgnoreIfException();

                // Notify the user of the error
                MessageBox.Show($"An error occurred when shutting down the application. Error message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Flag that we are done closing the main window
                DoneClosing = true;

                // Close application
                Shutdown();
            });
        }
    }

    /// <summary>
    /// Refreshes the application jump list
    /// </summary>
    public void RefreshJumpList()
    {
        Dispatcher?.Invoke(() =>
        {
            try
            {
                if (Data.App_JumpListItemIDCollection == null)
                {
                    Logger.Warn("The jump could not refresh due to collection not existing");

                    return;
                }

                // Create a jump list
                new JumpList(AppVM.GetGames.
                        // Add only games which have been added
                        Where(x => x.IsAdded()).
                        // Get the items for each game
                        Select(x => x.GetManager().GetJumpListItems()).
                        // Select into single collection
                        SelectMany(x => x).
                        // Keep only the included items
                        Where(x => x.IsIncluded).
                        // Keep custom order
                        OrderBy(x => Data.App_JumpListItemIDCollection.IndexOf(x.ID)).
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

    #region Private Events

    /// <summary>
    /// Contains events to be called once after startup completes
    /// </summary>
    private event AsyncEventHandler<EventArgs>? LocalStartupComplete;

    #endregion

    #region Public Events

    // TODO: Remove this async event system in favor of something better using tasks
    /// <summary>
    /// Occurs on startup, after the main window has been loaded
    /// </summary>
    public event AsyncEventHandler<EventArgs>? StartupComplete
    {
        add
        {
            using (StartupEventsCalledAsyncLock.Lock())
            {
                if (HasRunStartupEvents)
                    Task.Run(async () => await (value?.Invoke(this, EventArgs.Empty) ?? Task.CompletedTask));
                else
                    LocalStartupComplete += value;
            }
        }
        remove
        {
            if (!HasRunStartupEvents)
                LocalStartupComplete -= value;
        }
    }

    #endregion
}