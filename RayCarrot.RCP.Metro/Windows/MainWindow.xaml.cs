using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
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

        #endregion

        #region Event Handlers

        private async void MainWindow_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            await GamesPage.ViewModel.RefreshAsync();
        }

        private async void MainWindow_OnClosingAsync(object sender, CancelEventArgs e)
        {
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

                // TODO: Clean the temp

                // Save all user data
                await RCFRCP.App.SaveUserDataAsync();

                // Close application
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                // Attempt to log the exception, ignoring any exceptions thrown
                new Action(() => ex.HandleError("Closing main window")).IgnoreIfException();

                // Notify the user of the error
                MessageBox.Show($"An error occured when shutting down the application. Error message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Close application
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}