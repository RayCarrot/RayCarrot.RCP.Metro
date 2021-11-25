#nullable disable
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

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
        Services.App.RefreshRequired += AppGameRefreshRequiredAsync;
        Loaded += MainWindow_Loaded;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    /// <summary>
    /// Refreshes the enabled state of the backup page based on if there are any games available
    /// </summary>
    private void RefreshBackupPageEnabled() => Dispatcher?.Invoke(() =>
    {
        if (BackupPageTab == null)
            return;

        try
        {
            BackupPageTab.IsEnabled = Services.Data.Game_Games?.Any() ?? false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Refreshing backup page enabled");
            BackupPageTab.IsEnabled = true;
        }
    });

    #endregion

    #region Public Methods

    public void UpdateMinSize(bool updateWidth, bool updateHeight)
    {
        // If no child windows are open we use the default minimum sizes
        if (!ChildWindowInstance.OpenChildWindows.Any())
        {
            MinWidth = DefaultMinWidth;
            MinHeight = DefaultMinHeight;
            return;
        }

        // If there are child windows open we want to limit the minimum size to avoid resizing smaller than the open child windows
        if (updateWidth)
            // Get the max width of all the child windows. Include the padding on both sides and an additional margin to make sure
            // the drop shadow and other elements are included
            MinWidth = ChildWindowInstance.OpenChildWindows.
                Max(x => (x.IsMaximized ? x.MinContentWidth : x.ActualContentWidth) + x.Padding.Left + x.Padding.Right + 20);

        // Do the same for the height, but add an extra margin for the title bar
        if (updateHeight)
            MinHeight = ChildWindowInstance.OpenChildWindows.
                Max(x => (x.IsMaximized ? x.MinContentHeight : x.ActualContentHeight) + x.Padding.Top + x.Padding.Bottom + 80);
    }

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
                     OfType<System.Windows.Controls.TabItem>().
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