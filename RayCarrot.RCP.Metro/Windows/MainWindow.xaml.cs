using System;
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
        }

        #endregion

        #region Event Handlers

        private void App_RefreshRequired(object sender, EventArgs e)
        {
            // Disable the backup page tab when there are no games
            Dispatcher.Invoke(() => BackupPageTab.IsEnabled = RCFRCP.Data.Games.Any());
        }

        private async void MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            // Set up the secret code manager
            SecretCodeManager.Setup();

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

        private void MinimizeToTrayButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Window window in App.Current.Windows)
                window.Hide();

            TaskbarIcon.Visibility = Visibility.Visible;

            RCF.Logger.LogInformationSource("The program has been minimized to tray");
        }

        private void TaskbarIcon_Show_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Window window in App.Current.Windows)
                window.Show();

            TaskbarIcon.Visibility = Visibility.Collapsed;

            RCF.Logger.LogInformationSource("The program has been shown from the tray icon");
        }

        private void TaskbarIcon_Close_OnClick(object sender, RoutedEventArgs e)
        {
            RCF.Logger.LogInformationSource("The program is being closed from the tray icon");

            Close();
        }

        #endregion
    }
}