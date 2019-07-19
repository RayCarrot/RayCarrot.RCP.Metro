using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;

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
            if (CommonPaths.UpdaterFilePath.FileExists)
            {
                int retryTime = 0;

                // Wait until we can write to the file (i.e. it closing after an update)
                while (!RCFRCP.File.CheckFileWriteAccess(CommonPaths.UpdaterFilePath))
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
                    RCFRCP.File.DeleteFile(CommonPaths.UpdaterFilePath);

                    RCFCore.Logger?.LogInformationSource($"The updater has been removed");
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Removing updater");
                }
            }

            // Check for updates
            if (RCFRCP.Data.AutoUpdate)
                await RCFRCP.App.CheckForUpdatesAsync(false);
        }

        private void MinimizeToTrayButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Window window in App.Current.Windows)
                window.Hide();

            TaskbarIcon.Visibility = Visibility.Visible;

            RCFCore.Logger?.LogInformationSource("The program has been minimized to tray");
        }

        private void TaskbarIcon_Show_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Window window in App.Current.Windows)
                window.Show();

            TaskbarIcon.Visibility = Visibility.Collapsed;

            RCFCore.Logger?.LogInformationSource("The program has been shown from the tray icon");
        }

        private void TaskbarIcon_Close_OnClick(object sender, RoutedEventArgs e)
        {
            RCFCore.Logger?.LogInformationSource("The program is being closed from the tray icon");

            Close();
        }

        #endregion
    }
}