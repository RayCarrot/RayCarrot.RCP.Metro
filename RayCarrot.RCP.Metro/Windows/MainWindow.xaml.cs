using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.WPF;

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
            RCFRCP.App.RefreshRequired += AppGameRefreshRequiredAsync;
            Loaded += MainWindow_Loaded;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes the enabled state of the backup page based on if there are any games available
        /// </summary>
        private void RefreshBackupPageEnabled() => Dispatcher?.Invoke(() =>
        {
            try
            {
                BackupPageTab.IsEnabled = RCFRCP.Data.Games?.Any() ?? false;
            }
            catch (Exception ex)
            {
                ex.HandleError("Refreshing backup page enabled");
                BackupPageTab.IsEnabled = true;
            }
        });

        #endregion

        #region Event Handlers

        private Task AppGameRefreshRequiredAsync(object sender, RefreshRequiredEventArgs e)
        {
            // Disable the backup page tab when there are no games
            if (e.GameCollectionModified)
                RefreshBackupPageEnabled();

            return Task.CompletedTask;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshBackupPageEnabled();
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

        public static ObservableCollection<TestVM> TestVMs { get; } = new ObservableCollection<TestVM>()
        {
            new TestVM("Rayman Games"),
            new TestVM("Rabbids Games"),
            new TestVM("Spin-offs")
        };
    }

    public class TestVM : BaseRCPViewModel
    {
        public TestVM(string header)
        {
            Header = header;
        }

        public string Header { get; }

        public ObservableCollection<GameDisplayViewModel> Games { get; } = new ObservableCollection<GameDisplayViewModel>();
    }
}