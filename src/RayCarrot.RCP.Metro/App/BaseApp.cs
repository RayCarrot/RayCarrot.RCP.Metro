using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using RayCarrot.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NLog;
using NLog.Targets;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A base <see cref="Application"/> to inherit from
    /// </summary>
    public abstract class BaseApp : Application
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="useMutex">Indicates if a <see cref="Mutex"/> should be used to only allow a single instance of the application.
        /// This requires a valid GUID in the entry assembly.</param>
        /// <param name="splashScreenResourceName">The resource name for a splash screen if one is to be used</param>
        protected BaseApp(bool useMutex, string splashScreenResourceName = null)
        {
            // Create properties
            StartupEventsCalledAsyncLock = new AsyncLock();
            HasRunStartupEvents = false;

#if DEBUG
            // Create the startup timer and start it
            AppStartupTimer = new Stopwatch();
            AppStartupTimer.Start();

            StartupTimeLogs = new List<string>();
#endif

            LogStartupTime("BaseApp: Showing splash screen");

            // Create and show the splash screen if one is to be used
            if (splashScreenResourceName != null)
            {
                SplashScreenFadeout = TimeSpan.MinValue;
                SplashScreen = new SplashScreen(splashScreenResourceName);
                SplashScreen.Show(false);
            }

            // Subscribe to events
            Startup += BaseRCFApp_Startup;
            DispatcherUnhandledException += BaseRCFApp_DispatcherUnhandledException;
            Exit += BaseRCFApp_Exit;

            LogStartupTime("BaseApp: Checking Mutex");

            if (useMutex)
            {
                try
                {
                    var entry = Assembly.GetEntryAssembly();

                    if (entry == null)
                        throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if no valid entry assembly is found");

                    // Use mutex to only allow one instance of the application at a time
                    Mutex = new Mutex(false, "Global\\" + ((GuidAttribute)entry.GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if the entry assembly does not have a valid GUID identifier", ex);
                }
            }

            LogStartupTime("BaseApp: Construction finished");
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private Properties

        /// <summary>
        /// The splash screen, if one is used
        /// </summary>
        private SplashScreen SplashScreen { get; }

        /// <summary>
        /// The mutex
        /// </summary>
        private Mutex Mutex { get; }

        /// <summary>
        /// Indicates if the startup events have run
        /// </summary>
        protected bool HasRunStartupEvents { get; set; }

        /// <summary>
        /// Indicates if the main window is currently closing
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// Indicates if the main window is done closing and can be closed
        /// </summary>
        private bool DoneClosing { get; set; }

        /// <summary>
        /// The timer for the application startup
        /// </summary>
        private Stopwatch AppStartupTimer { get; }

        /// <summary>
        /// The startup time logs to log once the app has started to improve performance
        /// </summary>
        private List<string> StartupTimeLogs { get; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The fadeout for the splash screen
        /// </summary>
        protected TimeSpan SplashScreenFadeout { get; set; }

        /// <summary>
        /// Async lock for calling the startup events
        /// </summary>
        protected AsyncLock StartupEventsCalledAsyncLock { get; }

        /// <summary>
        /// The service provider for this application
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the <see cref="Application"/> object for the current <see cref="AppDomain"/> as a <see cref="BaseApp"/>.
        /// </summary>
        public new static BaseApp Current => Application.Current as BaseApp;

        /// <summary>
        /// The common application data, or null if not available
        /// </summary>
        public IAppInstanceData InstanceData => GetService<IAppInstanceData>();

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the application startup
        /// </summary>
        /// <param name="args">The launch arguments</param>
        private async void AppStartupAsync(string[] args)
        {
            LogStartupTime("Startup: App startup begins");

            // Set the shutdown mode to avoid any license windows to close the application
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Make sure the application can launch
            if (!await InitialSetupAsync(args))
            {
                Shutdown();
                return;
            }

            LogStartupTime("Startup: Initial setup has been verified");

            // Set up the app data and services
            SetupAppData(args);

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
            Logger.Debug("Entry assembly path: {0}", Assembly.GetEntryAssembly()?.Location);

            LogStartupTime("Startup: Debug info has been logged");

            // Run startup
            await OnSetupAsync(args);

            LogStartupTime("Startup: Startup has run");

            // Get the main window
            var mainWindow = GetMainWindow();

            LogStartupTime("Startup: Main window has been created");

            // Subscribe to events
            mainWindow.Loaded += MainWindow_LoadedAsync;
            mainWindow.Closing += MainWindow_ClosingAsync;
            mainWindow.Closed += MainWindow_Closed;

            // Close splash screen
            CloseSplashScreen();

            // Show the main window
            mainWindow.Show();

            // Set the shutdown mode
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        /// <summary>
        /// Sets up the application data and services
        /// </summary>
        private void SetupAppData(string[] args)
        {
            LogStartupTime("AppData: Setting up application data");

            // Set up the services
            var services = GetServices(args);

            // If there is no app data we add a default one
            if (services.All(x => x.ServiceType != typeof(IAppInstanceData)))
                services.AddSingleton<IAppInstanceData>(new AppInstanceData());

            LogStartupTime("AppData: Building app service provider");

            // Built the service provider
            ServiceProvider = services.BuildServiceProvider();

            // Retrieve arguments
            InstanceData.Arguments = args;

            // Log that the build is complete
            Logger.Info("The service provider has been built with {0} services", services.Count);

            LogStartupTime("AppData: Application data and services have been setup");
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Logs the startup time
        /// </summary>
        /// <param name="logDescription">The log description</param>
        [Conditional("DEBUG")]
        protected void LogStartupTime(string logDescription)
        {
            StartupTimeLogs.Add($"Startup: {AppStartupTimer.ElapsedMilliseconds} ms - {logDescription}");
        }

        /// <summary>
        /// Closes the splash screen if showing
        /// </summary>
        protected void CloseSplashScreen()
        {
            // Close splash screen
            SplashScreen?.Close(SplashScreenFadeout);
        }

        #endregion

        #region Event Handlers

        private void BaseRCFApp_Startup(object sender, StartupEventArgs e)
        {
            LogStartupTime("WPF Startup: Startup event called");

            try
            {
                if (Mutex != null && !Mutex.WaitOne(0, false))
                {
                    OnOtherInstanceFound(e.Args);
                    Shutdown();
                    return;
                }
            }
#pragma warning disable 168
            catch (AbandonedMutexException _)
#pragma warning restore 168
            {
                // Break if debugging
                Debugger.Break();
            }

            AppStartupAsync(e.Args);
        }

        private void BaseRCFApp_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                // Log the exception
                Logger.Fatal(e?.Exception, "Unhandled exception");

                // Get the path to log to
                string logPath = Path.Combine(Directory.GetCurrentDirectory(), "crashlog.txt");

                // Write log
                File.WriteAllLines(logPath, LogManager.Configuration.FindTargetByName<MemoryTarget>("memory")?.Logs ?? new string[]
                {
                    "Service not available",
                    Environment.NewLine,
                    e?.Exception?.ToString()
                });

                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e?.Exception?.Message}" +
                                $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}A crash log has been created under {logPath}.",
                    "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e?.Exception?.Message}",
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
            }
        }

        private void BaseRCFApp_Exit(object sender, ExitEventArgs e)
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
            AppStartupTimer.Stop();

            // Log all startup time logs
            foreach (string log in StartupTimeLogs)
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

                // Inform that startup events have run
                StartupEventsCompleted?.Invoke(this, EventArgs.Empty);

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
            // If ran RCF closing, ignore
            if (DoneClosing)
                return;

            // Cancel the native closing
            e.Cancel = true;

            // Don't close if the close button is disabled
            if (sender is MetroWindow {IsCloseButtonEnabled: false})
                return;

            // If already is closing, ignore
            if (IsClosing)
                return;

            Logger.Info("The main window is closing...");

            // Shut down the app
            await ShutdownRCFAppAsync(false);
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected abstract Window GetMainWindow();

        /// <summary>
        /// Gets the services to use for the application
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>The services to use</returns>
        protected abstract IServiceCollection GetServices(string[] args);

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// An optional custom setup to override
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>The task</returns>
        protected virtual Task OnSetupAsync(string[] args)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// An optional method to override which runs when closing
        /// </summary>
        /// <param name="mainWindow">The main Window of the application</param>
        /// <returns>The task</returns>
        protected virtual Task OnCloseAsync(Window mainWindow)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to run when another instance of the program is found running
        /// </summary>
        /// <param name="args">The launch arguments</param>
        protected virtual void OnOtherInstanceFound(string[] args)
        {

        }

        /// <summary>
        /// Optional initial setup to run. Can be used to check if the environment is valid
        /// for the application to run or for the user to accept the license.
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>True if the setup finished successfully or false if the application has to shut down</returns>
        protected virtual Task<bool> InitialSetupAsync(string[] args)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Disposes any disposable application objects
        /// </summary>
        protected virtual void Dispose()
        {
            Mutex?.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shuts down the RCF app
        /// </summary>
        /// <param name="forceShutDown">Indicates if the app should be forced to shut down</param>
        /// <returns>The task</returns>
        public async Task ShutdownRCFAppAsync(bool forceShutDown)
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
                    foreach (ChildWindow childWindow in ChildWindowInstance.OpenChildWindows.OrderBy(x => x.IsModal ? 0 : 1).ToArray())
                        childWindow.Close();

                    // Yield so that the child windows fully close before we do the next check
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                    // Make sure all other windows have been closed unless forcing a shut down
                    if (!forceShutDown && (Windows.Count > 1 || ChildWindowInstance.OpenChildWindows.Any()))
                    {
                        Logger.Info("The shutdown was canceled due to one or more windows still being open");

                        IsClosing = false;
                        return;
                    }

                    // Run shut down code
                    await OnCloseAsync(MainWindow);

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
        /// Gets a service from the application service provider
        /// </summary>
        /// <typeparam name="T">The type of service</typeparam>
        /// <returns>The requested object of the specified type or null if not available</returns>
        public T GetService<T>() where T : class => ServiceProvider?.GetService<T>();

        #endregion

        #region Protected Events

        /// <summary>
        /// Contains events to be called once after startup completes
        /// </summary>
        protected event AsyncEventHandler<EventArgs> LocalStartupComplete;

        /// <summary>
        /// Occurs when all event handlers subscribed to <see cref="StartupComplete"/> have finished
        /// </summary>
        protected event EventHandler StartupEventsCompleted;

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs on startup, after the main window has been loaded
        /// </summary>
        public event AsyncEventHandler<EventArgs> StartupComplete
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
}