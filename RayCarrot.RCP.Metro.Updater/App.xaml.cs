using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro;

namespace RayCarrot.RCP.Metro.Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructor

        public App()
        {
            WC = new WebClient();

            // Get temp file names
            LocalTempPath = Path.GetTempFileName();
            ServerTempPath = Path.GetTempFileName();
        }

        #endregion

        #region Event Handlers

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            //
            // Arguments:
            //  - RCP filePath (string)
            //  - Dark mode (bool)
            //  - User level (UserLevel)
            //  - Is beta update (bool)
            //  - Culture (string)
            //

            // Retrieve the arguments
            string[] args = e.Args;

            // Make sure we have 5 arguments
            if (args.Length != 5)
            {
                ShutdownApplication("The number of launch arguments need to be 5");
                return;
            }

            // Attempt to get the file path
            RCPFilePath = args[0];

            // Make sure the file exists
            if (!File.Exists(RCPFilePath))
            {
                ShutdownApplication("The file specified in the first launch argument does not exist");
                return;
            }

            // Get the dark mode value
            var darkMode = !Boolean.TryParse(args[1], out bool dm) || dm;

            // Set the app theme
            ThemeManager.ChangeAppTheme(this, $"Base{(darkMode ? "Dark" : "Light")}");

            // Get the user level
            CurrentUserLevel = Enum.TryParse(args[2], out UserLevel ule) ? ule : UserLevel.Normal;

            // Get the beta update flag
            IsBetaUpdate = !Boolean.TryParse(args[3], out bool gbu) || gbu;

            try
            {
                // Get the culture info
                var ci = new CultureInfo(args[4]);

                // Update the current thread cultures
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = ci;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error setting culture", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Create and show the main window
            new MainWindow().Show();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShutdownApplication("Unhandled exception", e.Exception);
            WC?.Dispose();
            ClearTemp();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            WC?.Dispose();
            ClearTemp();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shuts down the application with an optional exception
        /// </summary>
        /// <param name="debugMessage">A debug message explaining the reason for the shutdown</param>
        /// <param name="ex">The exception which caused the shutdown</param>
        public void ShutdownApplication(string debugMessage, Exception ex = null)
        {
            if (Dispatcher == null)
            {
                Shutdown();
                return;
            }

            Dispatcher.Invoke(() =>
            {
                var win = new ErrorWindow(debugMessage, ex)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                if (win != MainWindow && MainWindow != null)
                {
                    win.Owner = MainWindow;
                    win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }

                win.ShowDialog();

                Shutdown();
            });
        }

        /// <summary>
        /// Clears the temporary files for this program
        /// </summary>
        public void ClearTemp()
        {
            if (File.Exists(ServerTempPath))
            {
                try
                {
                    File.Delete(ServerTempPath);
                }
                catch
                {
                    // Ignore exception
                }
            }

            if (File.Exists(LocalTempPath))
            {
                try
                {
                    File.Delete(LocalTempPath);
                }
                catch
                {
                    // Ignore exception
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current user level
        /// </summary>
        public UserLevel CurrentUserLevel { get; set; }

        /// <summary>
        /// The Rayman Control Panel file path
        /// </summary>
        public string RCPFilePath { get; set; }

        /// <summary>
        /// The temporary path for the server update
        /// </summary>
        public string ServerTempPath { get; }

        /// <summary>
        /// The temporary path for the local program
        /// </summary>
        public string LocalTempPath { get; }

        /// <summary>
        /// Indicates if the update is a beta update
        /// </summary>
        public bool IsBetaUpdate { get; set; }

        /// <summary>
        /// The web client for downloading the update
        /// </summary>
        public WebClient WC { get; }

        /// <summary>
        /// The current stage in the update process
        /// </summary>
        public UpdateStage Stage { get; set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The current application
        /// </summary>
        public new static App Current => Application.Current as App;

        #endregion

        #region Public Constants

        public const string UpdateManifestUrl = "http://raycarrot.ylemnova.com/RCP/RCP_Metro_Manifest.json";

        #endregion
    }
}