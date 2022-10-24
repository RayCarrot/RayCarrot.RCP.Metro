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

    public MainWindow(AppViewModel app, AppUserData data, MainWindowViewModel viewModel)
    {
        InitializeComponent();

        // Set properties
        App = app;
        Data = data;
        DataContext = ViewModel = viewModel;

        // Subscribe to events
        App.RefreshRequired += AppGameRefreshRequiredAsync;
        Loaded += MainWindow_Loaded;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private AppViewModel App { get; }
    private AppUserData Data { get; }

    #endregion

    #region Public Methods

    public MainWindowViewModel ViewModel { get; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Refreshes the enabled state of the progression page based on if there are any games available
    /// </summary>
    private void RefreshProgressionPageEnabled() => Dispatcher?.Invoke(() =>
    {
        if (ProgressionPageTab == null)
            return;

        try
        {
            ProgressionPageTab.IsEnabled = Data.Game_InstalledGames?.Any() ?? false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Refreshing progression page enabled");
            ProgressionPageTab.IsEnabled = true;
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
        // Disable the progression page tab when there are no games
        if (e.GameCollectionModified)
            RefreshProgressionPageEnabled();

        return Task.CompletedTask;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Refresh if the progression page should be enabled
        RefreshProgressionPageEnabled();

        // Set the data context for each overflow item
        foreach (BasePage page in PageTabControl.Items.
                     // Get all tab items
                     OfType<System.Windows.Controls.TabItem>().
                     // Get the content of the tab items
                     Select(x => x.Content).
                     // Only get base pages
                     OfType<BasePage>().
                     // Make sure there is an overflow menu
                     Where(x => x.OverflowMenu != null))
        {
            // Set the data context
            page.OverflowMenu!.DataContext = page.DataContext;
        }
    }

    #endregion
}