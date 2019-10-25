using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RayCarrot.Extensions;

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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // Make sure we got a page
            if (!(PageTabControl.SelectedContent is IBasePage page))
                return;

            // Make sure there is an overflow menu
            if (page.OverflowMenu == null)
                return;

            // Set the button
            OverflowMenuButton = sender.CastTo<Button>();

            // Open the overflow menu
            page.OverflowMenu.IsOpen = true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Refresh if the backup page should be enabled
            RefreshBackupPageEnabled();

            // Subscribe to when the overflow menu first loads for each page
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
                // Get the overflow menu
                var menu = page.OverflowMenu;

                // Set the data context
                menu.DataContext = (page as FrameworkElement)?.DataContext;

                // Set the placement
                menu.Placement = PlacementMode.Absolute;

                // Subscribe to when loaded
                menu.Loaded += (s, ee) =>
                {
                    // Get the point on screen
                    var point = OverflowMenuButton.PointToScreen(
                        new Point(0 - menu.ActualWidth + OverflowMenuButton.ActualWidth, OverflowMenuButton.ActualHeight));

                    // Set the offsets with a margin of 10a
                    menu.HorizontalOffset = point.X - 10;
                    menu.VerticalOffset = point.Y + 10;
                };
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The overflow menu button
        /// </summary>
        protected Button OverflowMenuButton { get; set; }

        #endregion
    }
}