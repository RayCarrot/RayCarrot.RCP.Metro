using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Service for managing the application startup
/// </summary>
public class StartupManager
{
    #region Constructor

    public StartupManager(
        LaunchArguments args, 
        LoggerManager loggerManager,
        AppDataManager appDataManager,
        AppUserData data, 
        AppViewModel appViewModel, 
        JumpListManager jumpListManager, 
        DeployableFilesManager deployableFilesManager, 
        GamesManager gamesManager)
    {
        Args = args ?? throw new ArgumentNullException(nameof(args));
        LoggerManager = loggerManager ?? throw new ArgumentNullException(nameof(loggerManager));
        AppDataManager = appDataManager ?? throw new ArgumentNullException(nameof(appDataManager));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        AppViewModel = appViewModel ?? throw new ArgumentNullException(nameof(appViewModel));
        JumpListManager = jumpListManager ?? throw new ArgumentNullException(nameof(jumpListManager));
        DeployableFilesManager = deployableFilesManager ?? throw new ArgumentNullException(nameof(deployableFilesManager));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private TimeSpan SplashScreenFadeoutTime => TimeSpan.FromMilliseconds(200);
    private SplashScreen? SplashScreen { get; set; }
    private const string SplashScreenResourceName = "Files/Splash Screen.png";

    private LaunchArguments Args { get; }
    private LoggerManager LoggerManager { get; }
    private AppDataManager AppDataManager { get; }
    private AppUserData Data { get; }
    private AppViewModel AppViewModel { get; }
    private JumpListManager JumpListManager { get; }
    private DeployableFilesManager DeployableFilesManager { get; }
    private GamesManager GamesManager { get; }

    #endregion

    #region Private Methods

    private void CloseSplashScreen()
    {
        SplashScreen?.Close(SplashScreenFadeoutTime);
        SplashScreen = null;
    }

    private void InitEnvironment()
    {
        // Hard code the current directory to avoid any issues with the
        // application launching from other locations than its own
        string? assemblyPath = Assembly.GetEntryAssembly()?.Location;
        string? assemblyDir = Path.GetDirectoryName(assemblyPath);

        if (assemblyDir != null)
            Directory.SetCurrentDirectory(assemblyDir);
    }

    private void InitLogging()
    {
        LoggerManager.Initialize(Args);

        // Log the current environment
        try
        {
            Logger.Info("Current platform: {0}", Environment.OSVersion.VersionString);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Logging environment details");
        }
    }

    private void LoadAppData()
    {
        AppDataManager.Load();
    }

    private void InitXAML()
    {
        // Set the theme
        App.Current.SetTheme(Data.Theme_DarkMode, Data.Theme_SyncTheme);
    }

    private void InitLocalization()
    {
        // Apply the current culture if defaulted
        if (Data.App_CurrentCulture == LocalizationManager.DefaultCulture.Name)
            LocalizationManager.SetCulture(LocalizationManager.DefaultCulture.Name);
    }

    private void InitJumpList()
    {
        JumpListManager.Initialize();
    }

    private void CheckLaunchArgs()
    {
        Logger.Trace("Launch arguments:");
        foreach (string arg in Args.Args)
            Logger.Trace(arg);

        // Check for user level argument
        if (Args.HasArg("-ul", out string? ul))
        {
            try
            {
                Data.App_UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting user level from args");
            }
        }

        // NOTE: Starting with the updater 3.0.0 (available from 4.5.0) this is no longer used.
        //       It must however be maintained for legacy support (i.e. updating to version 4.5.0+
        //       using an updater below 3.0.0)
        // Check for updater install argument
        if (Args.HasArg("-install", out string? updateFile))
        {
            try
            {
                if (File.Exists(updateFile))
                {
                    new FileSystemPath(updateFile).DeleteFile();
                    Logger.Info("The updater was deleted");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deleting updater");
            }
        }
    }

    private FileLaunchHandler? CheckFileLaunch()
    {
        // Check if the app was opened from a file
        if (Args.FilePathArg != null)
            // Attempt to validate with an available file launch handler
            return FileLaunchHandler.GetHandler(Args.FilePathArg.Value);

        return null;
    }

    private URILaunchHandler? CheckURILaunch()
    {
        // Check if the app was opened from a URI
        if (Args.HasArgs)
            return URILaunchHandler.GetHandler(Args.Args[0]);

        return null;
    }

    private void InitWebProtocol()
    {
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
    }

    private void CheckFirstLaunch()
    {
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
    }

    private async Task ValidateGamesAsync()
    {
        // Keep track of removed games
        List<GameInstallation> removed = new();

        // TODO-14: Only show single message to user
        
        // Make sure every game is valid
        foreach (GameInstallation gameInstallation in GamesManager.EnumerateInstalledGames())
        {
            // Check if it's valid
            if (await gameInstallation.GameDescriptor.IsValidAsync(gameInstallation.InstallLocation))
                continue;

            // Show message
            await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.GameNotFound, gameInstallation.GameDescriptor.DisplayName), 
                Resources.GameNotFoundHeader, MessageType.Error);

            // Add to removed games
            removed.Add(gameInstallation);

            Logger.Info("The game {0} is being removed due to not being valid", gameInstallation.Id);
        }

        // Remove the games
        await Services.Games.RemoveGamesAsync(removed);
    }

    private async Task PostUpdateAsync()
    {
        Logger.Info("Current version is {0}", AppViewModel.CurrentAppVersion);

        // Check if it's a new version
        if (Data.App_LastVersion < AppViewModel.CurrentAppVersion)
        {
            await AppDataManager.PostUpdateAsync(Data.App_LastVersion);

            // Refresh the jump list
            JumpListManager.Refresh();

            // Close the splash screen
            CloseSplashScreen();

            // Show app news
            await Services.UI.ShowAppNewsAsync();

            // Update the last version
            Data.App_LastVersion = AppViewModel.CurrentAppVersion;

            try
            {
                // Clear the temp folder
                AppFilePaths.TempPath.DeleteDirectory();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Clearing app temp folder");
            }
        }
        // Check if it's a lower version than previously recorded
        else if (Data.App_LastVersion > AppViewModel.CurrentAppVersion)
        {
            Logger.Warn("A newer version ({0}) has been recorded in the application data", Data.App_LastVersion);

            if (!Data.Update_DisableDowngradeWarning)
                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.DowngradeWarning, AppViewModel.CurrentAppVersion,
                    Data.App_LastVersion), Resources.DowngradeWarningHeader, MessageType.Warning);
        }
    }

    private void ShowAppWindow<AppWindow>(Func<AppWindow> createWindow)
        where AppWindow : Window
    {
        // Create the main window
        AppWindow appWindow = createWindow();

        // Load previous state
        Data.UI_WindowState?.ApplyToWindow(appWindow);

        appWindow.PreviewKeyDown += async (_, e) => await SecretCodeManager.AddKeyAsync(e.Key);
        
        appWindow.Closing += AppWindow_Closing;
        appWindow.Closed += AppWindow_Closed;

        // Close the splash screen
        CloseSplashScreen();

        // Show the main window
        appWindow.Show();

        // Static event handlers
        static async void AppWindow_Closing(object sender, CancelEventArgs e)
        {
            // NOTE: When the actual app shutdown is called this even will be invoked
            //       which will cause the below code to run twice if the user closes
            //       the window. However that doesn't matter since e.Cancel is ignored
            //       the second time and ShutdownAppAsync won't run while shutting down.

            // Cancel the native closing
            e.Cancel = true;

            // Don't close if the close button is disabled (such as from F4)
            if (sender is MetroWindow { IsCloseButtonEnabled: false })
                return;

            Logger.Info("The app window is closing...");

            // Shut down the app
            await App.Current.ShutdownAppAsync(false);
        }

        static void AppWindow_Closed(object sender, EventArgs e)
        {
            // Make sure the app shuts down. It should already
            // be shutting down at this point in which case
            // this will be ignored.
            App.Current.Shutdown();
        }
    }

    private Task CleanDeployedFilesAsync()
    {
        // Clean up deployed files
        return Task.Run(DeployableFilesManager.CleanupFilesAsync);
    }

    private async Task ShowLogViewerAsync()
    {
        // Show log viewer if available
        if (LoggerManager.IsLogViewerAvailable)
        {
            await App.Current.Dispatcher.InvokeAsync(() => LogViewer.Open(LoggerManager.LogViewerViewModel));
            App.Current.MainWindow?.Focus();
        }
    }

    private Task RunGameFinderAsync()
    {
        // Check for installed games
        if (Data.Game_AutoLocateGames)
            return Task.Run(AppViewModel.RunGameFinderAsync);
        else
            return Task.CompletedTask;
    }

    private Task CheckForUpdatesAsync()
    {
        // Check for updates
        if (Data.Update_AutoUpdate)
            return Task.Run(() => AppViewModel.CheckForUpdatesAsync(false));
        else
            return Task.CompletedTask;
    }

    #endregion

    #region Public Methods

    public async Task RunAsync<AppWindow>(bool isFullStartup, Func<AppWindow> createWindow)
        where AppWindow : Window
    {
        try
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Create the splash screen and show it
            SplashScreen = new SplashScreen(SplashScreenResourceName);
            SplashScreen.Show(false);

            AppViewModel.IsStartupRunning = true;

            InitEnvironment();
            InitLogging();
            Logger.Debug("Startup {0} ms: Initialized environment and logging", sw.ElapsedMilliseconds);

            LoadAppData();
            Logger.Debug("Startup {0} ms: Loaded app data", sw.ElapsedMilliseconds);

            InitXAML();
            InitLocalization();
            InitJumpList();
            Logger.Debug("Startup {0} ms: Initialized XAML, localization and jump list", sw.ElapsedMilliseconds);

            CheckLaunchArgs();
            FileLaunchHandler? fileLaunchHandler = CheckFileLaunch();
            URILaunchHandler? uriLaunchHandler = CheckURILaunch();
            InitWebProtocol();
            Logger.Debug("Startup {0} ms: Checked launch arguments and initialized web protocol", sw.ElapsedMilliseconds);

            // The file or URI launch can optionally disable the full startup
            if (fileLaunchHandler?.DisableFullStartup == true ||
                uriLaunchHandler?.DisableFullStartup == true)
                isFullStartup = false;

            if (isFullStartup)
            {
                CheckFirstLaunch();
                await ValidateGamesAsync();
                await PostUpdateAsync();
                Logger.Debug("Startup {0} ms: Checked first launch, validated games & ran post-update", sw.ElapsedMilliseconds);
            }

            ShowAppWindow<AppWindow>(createWindow);
            Logger.Debug("Startup {0} ms: Showed app window", sw.ElapsedMilliseconds);

            // Show the log viewer if set to do so
            await ShowLogViewerAsync();
            Logger.Debug("Startup {0} ms: Optionally showed log viewer", sw.ElapsedMilliseconds);

            // Invoke the file or uri launch handler if we have one
            fileLaunchHandler?.Invoke(Args.FilePathArg!.Value, LaunchArgHandler.State.Startup);
            uriLaunchHandler?.Invoke(Args.Args[0], LaunchArgHandler.State.Startup);

            // Start receiving arguments from potentially new process
            Args.StartReceiveArguments();

            // The following startup actions can run in the background after the app window has opened
            if (isFullStartup)
            {
                // Start by cleaning deployed files
                await CleanDeployedFilesAsync();
                Logger.Debug("Startup {0} ms: Cleaned deployed files", sw.ElapsedMilliseconds);

                // Run the following actions in parallel
                await Task.WhenAll(new[]
                {
                    RunGameFinderAsync(),
                    CheckForUpdatesAsync(),
                });
                Logger.Debug("Startup {0} ms: Ran game finder and checked for updates", sw.ElapsedMilliseconds);
            }

            sw.Stop();

            Logger.Info("Finished running startup in {0} ms", sw.ElapsedMilliseconds);
        }
        finally
        {
            AppViewModel.IsStartupRunning = false;
            CloseSplashScreen();
        }
    }

    #endregion
}