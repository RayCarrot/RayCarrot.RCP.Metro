using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.WPF;

namespace RayCarrot.RCP.UI
{
    // TODO: Generalize app startup more

    /// <summary>
    /// The base WPF application for a Rayman Control Panel app
    /// </summary>
    /// <typeparam name="Win">The type of main window to create</typeparam>
    public abstract class BaseRCPApp<Win> : BaseRCFApp
        where Win : Window, new()
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
            StartupEventsCompleted += App_StartupEventsCompleted;
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected override Window GetMainWindow()
        {
            // Create the window
            var window = new Win();

            // Load previous state
            RCFRCPUI.Data?.WindowState?.ApplyToWindow(window);

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
            if (RCFRCPUI.Data != null)
                RCFRCPUI.Data.WindowState = WindowSessionState.GetWindowState(mainWindow);

            RCFCore.Logger?.LogInformationSource($"The application is exiting...");

            // Save all user data
            await RCFRCPUI.App.SaveUserDataAsync();
        }

        #endregion

        #region Event Handlers

        private static void App_StartupEventsCompleted(object sender, EventArgs e)
        {
            RCFRCPUI.App.IsStartupRunning = false;
        }

        #endregion
    }
}