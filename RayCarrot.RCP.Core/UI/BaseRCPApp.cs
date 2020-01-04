using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using RayCarrot.WPF.Metro;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// The base WPF application for a Rayman Control Panel app
    /// </summary>
    /// <typeparam name="Win">The type of main window to create</typeparam>
    /// <typeparam name="Data">The type of app user data</typeparam>
    /// <typeparam name="API">The type of API controller manager</typeparam>
    public abstract class BaseRCPApp<Win, Data, API> : BaseRCFApp
        where Win : Window, new()
        where Data : RCPAppUserData, new()
        where API : BaseAPIControllerManager, new()
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="useMutex">Indicates if a <see cref="Mutex"/> should be used to only allow a single instance of the application.
        /// This requires a valid GUID in the entry assembly.</param>
        /// <param name="splashScreenResourceName">The resource name for a splash screen if one is to be used</param>
        protected BaseRCPApp(bool useMutex, string splashScreenResourceName = null) : base(useMutex, splashScreenResourceName)
        {
            DataChangedHandlerAsyncLock = new AsyncLock();

            StartupEventsCompleted += App_StartupEventsCompleted;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock for <see cref="Data_PropertyChangedAsync"/>
        /// </summary>
        private AsyncLock DataChangedHandlerAsyncLock { get; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The app user data
        /// </summary>
        protected Data AppData { get; set; }

        /// <summary>
        /// Gets a new API controller instance
        /// </summary>
        protected API GetAPI => new API();

        #endregion

        #region Protected Override Methods

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
                await RCFData.UserDataCollection.AddUserDataAsync<Data>(RCFRCPC.Path.GetUserDataFile(typeof(Data)));

                RCFCore.Logger?.LogInformationSource($"The app user data has been loaded");
            }
            catch (Exception ex)
            {
                ex.HandleError($"Loading app user data");

                // NOTE: This is not localized due to the current culture not having been set at this point
                MessageBox.Show($"An error occurred reading saved app data. Some settings have been reset to their default values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Recreate the user data and reset it
                RCFData.UserDataCollection.Add(new Data());
                RCFRCPC.Data.Reset();
            }

            AppData = RCFRCPC.Data.CastTo<Data>();

            LogStartupTime("User data has been loaded");

            // Set the theme
            this.SetTheme(AppData.DarkMode);

            // Apply the current culture if defaulted
            if (AppData.CurrentCulture == RCFRCPC.Localization.DefaultCulture.Name)
                RCFRCPC.Localization.SetCulture(RCFRCPC.Localization.DefaultCulture.Name);

            // Listen to data binding logs
            WPFTraceListener.Setup(LogLevel.Warning);

            StartupComplete += BaseApp_StartupComplete_Miscellaneous_Async;
            StartupComplete += App_StartupComplete_Updater_Async;
            AppData.PropertyChanged += Data_PropertyChangedAsync;

            // Run basic startup
            await BasicStartupAsync();

            RCFCore.Logger?.LogInformationSource($"Current version is {RCFRCPC.App.CurrentAppVersion}");

            // Check if it's a new version
            if (AppData.LastVersion < RCFRCPC.App.CurrentAppVersion)
            {
                // Run post-update code
                await PostUpdateAsync();

                LogStartupTime("Post update has run");

                // Update the last version
                AppData.LastVersion = RCFRCPC.App.CurrentAppVersion;
            }
            // Check if it's a lower version than previously recorded
            else if (AppData.LastVersion > RCFRCPC.App.CurrentAppVersion)
            {
                RCFCore.Logger?.LogWarningSource($"A newer version ({AppData.LastVersion}) has been recorded in the application data");

                if (!AppData.DisableDowngradeWarning)
                    await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Core.Resources.DowngradeWarning, RCFRCPC.App.CurrentAppVersion,
                            AppData.LastVersion), Core.Resources.DowngradeWarningHeader, MessageType.Warning);
            }
        }

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected override Window GetMainWindow()
        {
            // Create the window
            var window = new Win();

            // Load previous state
            RCFRCPC.Data?.WindowState?.ApplyToWindow(window);

            return window;
        }

        /// <summary>
        /// An optional method to override which runs when closing
        /// </summary>
        /// <param name="mainWindow">The main Window of the application</param>
        /// <returns>The task</returns>
        protected override async Task OnCloseAsync(Window mainWindow)
        {
            // Save window state
            if (RCFRCPC.Data != null)
                RCFRCPC.Data.WindowState = WindowSessionState.GetWindowState(mainWindow);

            RCFCore.Logger?.LogInformationSource($"The application is exiting...");

            // Save all user data
            await RCFRCPC.App.SaveUserDataAsync();
        }

        /// <summary>
        /// Override to run when another instance of the program is found running
        /// </summary>
        /// <param name="args">The launch arguments</param>
        protected override void OnOtherInstanceFound(string[] args)
        {
            MessageBox.Show($"An instance of {GetAPI.AppDisplayName} is already running", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (BaseRCPAppViewModel.WindowsVersion < WindowsVersion.WinVista)
            {
                MessageBox.Show("Windows Vista or higher is required to run this application", "Error starting", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult(false);
            }

            // Make sure the license has been accepted
            if (!ShowLicense())
                return Task.FromResult(false);

            // Hard code the current directory
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Directory.GetCurrentDirectory());

            return Task.FromResult(true);
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Runs the basic startup, such as handling launch arguments
        /// </summary>
        /// <returns>The task</returns>
        protected virtual Task BasicStartupAsync()
        {
            LogStartupTime("Running API basic startup");

            // Check for reset argument
            if (RCFCore.Data.Arguments.Contains("-reset"))
                RCFRCPC.Data.Reset();

            // Check for user level argument
            if (RCFCore.Data.Arguments.Contains("-ul"))
            {
                try
                {
                    string ul = RCFCore.Data.Arguments[RCFCore.Data.Arguments.FindItemIndex(x => x == "-ul") + 1];
                    AppData.UserLevel = Enum.Parse(typeof(UserLevel), ul, true).CastTo<UserLevel>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Setting user level from args");
                }
            }

            // Update the application path
            FileSystemPath appPath = Assembly.GetEntryAssembly()?.Location;

            if (appPath != AppData.ApplicationPath)
            {
                AppData.ApplicationPath = appPath;

                RCFCore.Logger?.LogInformationSource("The application path has been updated");
            }

            LogStartupTime("API basic startup has run");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles a change in the app user data
        /// </summary>
        /// <param name="propertyName">The name of the property which changed</param>
        /// <returns>The task</returns>
        protected virtual Task AppUserDataChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(RCPAppUserData.DarkMode):
                    this.SetTheme(AppData.DarkMode);
                    break;

                case nameof(RCPAppUserData.ShowIncompleteTranslations):
                    RCFRCPC.Localization.RefreshLanguages(AppData.ShowIncompleteTranslations);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Shows the application license message and returns a value indicating if it was accepted
        /// </summary>
        /// <returns>True if it was accepted, false if not</returns>
        protected virtual bool ShowLicense()
        {
            try
            {
                // Get the license value, if one exists
                int regValue = Registry.GetValue(GetAPI.RegistryBaseKey, GetAPI.RegistryLicenseValue, 0)?.CastTo<int>() ?? 0;

                // Check if it has been accepted
                if (regValue == 1)
                    return true;

                // Create license popup dialog
                var ld = new LicenseDialog();

                // Close the splash screen
                CloseSplashScreen();

                // Show the dialog
                ld.ShowDialog();

                // Set Registry value if accepted
                if (ld.Accepted)
                    Registry.SetValue(GetAPI.RegistryBaseKey, GetAPI.RegistryLicenseValue, 1);

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
        /// Runs the post-update code if the version running is newer than the previous one
        /// </summary>
        protected virtual Task PostUpdateAsync() => Task.CompletedTask;

        #endregion

        #region Event Handlers

        private static void App_StartupEventsCompleted(object sender, EventArgs e)
        {
            RCFRCPC.App.IsStartupRunning = false;
        }

        private static async Task App_StartupComplete_Updater_Async(object sender, EventArgs eventArgs)
        {
            if (RCFRCPC.Path.UpdaterFile.FileExists)
            {
                int retryTime = 0;

                // Wait until we can write to the file (i.e. it closing after an update)
                while (!RCFRCPC.File.CheckFileWriteAccess(RCFRCPC.Path.UpdaterFile))
                {
                    retryTime++;

                    // Try for 2 seconds first
                    if (retryTime < 20)
                    {
                        RCFCore.Logger?.LogDebugSource($"The updater can not be removed due to not having write access. Retrying {retryTime}");

                        await Task.Delay(100);
                    }
                    // Now it's taking a long time... Try for 10 more seconds
                    else if (retryTime < 70)
                    {
                        RCFCore.Logger?.LogWarningSource($"The updater can not be removed due to not having write access. Retrying {retryTime}");

                        await Task.Delay(200);
                    }
                    // Give up and let the deleting of the file give an error message
                    else
                    {
                        RCFCore.Logger?.LogCriticalSource($"The updater can not be removed due to not having write access");
                        break;
                    }
                }

                try
                {
                    // Remove the updater
                    RCFRCPC.File.DeleteFile(RCFRCPC.Path.UpdaterFile);

                    RCFCore.Logger?.LogInformationSource($"The updater has been removed");
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Removing updater");
                }
            }

            // Check for updates
            if (RCFRCPC.Data.AutoUpdate)
                await RCFRCPC.App.CheckForUpdatesAsync(false);
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
                    MainWindow?.Focus();
                }
            });
        }

        private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            using (await DataChangedHandlerAsyncLock.LockAsync())
                await AppUserDataChanged(e.PropertyName);
        }

        #endregion
    }
}