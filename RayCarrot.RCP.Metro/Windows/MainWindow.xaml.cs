using System;
using System.ComponentModel;
using System.Windows;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The main application window
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Subscribe to events
            RCFRCP.App.RefreshRequired += App_RefreshRequired;
            Loaded += MainWindow_LoadedAsync;
            Loaded += MainWindow_Loaded2Async;

            if (RCFRCP.Data.WindowState == null)
                return;

            // Load previous state
            Height = RCFRCP.Data.WindowState.WindowHeight;
            Width = RCFRCP.Data.WindowState.WindowWidth;
            Left = RCFRCP.Data.WindowState.WindowLeft;
            Top = RCFRCP.Data.WindowState.WindowTop;
            WindowState = RCFRCP.Data.WindowState.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Indicates if the window is currently closing
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// Indicates if the window is done closing and can be closed
        /// </summary>
        private bool DoneClosing { get; set; }

        #endregion

        #region Event Handlers

        private void App_RefreshRequired(object sender, EventArgs e)
        {
            // Disable the backup page tab when there are no games
            Dispatcher.Invoke(() => BackupPageTab.IsEnabled = RCFRCP.Data.Games.Any());
        }

        private async void MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await GamesPage.ViewModel.RefreshAsync();
            App_RefreshRequired(null, null);

            // Save settings
            await RCFRCP.App.SaveUserDataAsync();

            // Check for installed games
            if (RCFRCP.Data.AutoLocateGames)
                await RCFRCP.App.RunGameFinderAsync();
        }

        private async void MainWindow_Loaded2Async(object sender, RoutedEventArgs e)
        {
            // Check for updates
            if (RCFRCP.Data.AutoUpdate)
                await RCFRCP.App.CheckForUpdatesAsync(false);
        }

        private async void MainWindow_OnClosingAsync(object sender, CancelEventArgs e)
        {
            if (DoneClosing)
                return;

            e.Cancel = true;

            if (IsClosing)
                return;

            IsClosing = true;

            try
            {
                RCF.Logger.LogInformationSource("The main window is closing...");

                // Save state
                RCFRCP.Data.WindowState = new WindowSessionState()
                {
                    WindowHeight = Height,
                    WindowWidth = Width,
                    WindowLeft = Left,
                    WindowTop = Top,
                    WindowMaximized = WindowState == WindowState.Maximized
                };

                RCF.Logger.LogInformationSource($"The application is exiting...");

                // Clean the temp
                RCFRCP.App.CleanTemp();

                // Save all user data
                await RCFRCP.App.SaveUserDataAsync();

                DoneClosing = true;

                // Close application
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                // Attempt to log the exception, ignoring any exceptions thrown
                new Action(() => ex.HandleError("Closing main window")).IgnoreIfException();

                // Notify the user of the error
                MessageBox.Show($"An error occured when shutting down the application. Error message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                DoneClosing = true;

                // Close application
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}