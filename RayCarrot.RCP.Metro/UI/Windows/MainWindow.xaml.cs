using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            // Refresh if the backup page should be enabled
            RefreshBackupPageEnabled();

            // Set the data context for each overflow item
            foreach (var page in PageTabControl.Items.
                // Get all tab items
                OfType<TabItem>().
                // Get the content of the tab items
                Select(x => x.Content).
                // Only get base pages
                OfType<IBasePage>().
                // Make sure there is an overflow menu
                Where(x => x.OverflowMenu != null))
            {
                // Set the data context
                page.OverflowMenu.DataContext = (page as FrameworkElement)?.DataContext;
            }
        }

        #endregion
    }
}