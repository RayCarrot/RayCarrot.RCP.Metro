using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The main application window
/// </summary>
public partial class MainWindow : BaseWindow, IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>
{
    #region Constructor

    public MainWindow(MainWindowViewModel viewModel, IMessenger messenger, GamesManager games)
    {
        InitializeComponent();

        // Set properties
        Games = games ?? throw new ArgumentNullException(nameof(games));
        DataContext = ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        // Register for messages
        messenger.RegisterAll(this);

        // Subscribe to events
        Loaded += MainWindow_Loaded;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private GamesManager Games { get; }

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
            ProgressionPageTab.IsEnabled = Games.AnyInstalledGames();
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

    #region Message Receivers

    void IRecipient<AddedGamesMessage>.Receive(AddedGamesMessage message) => RefreshProgressionPageEnabled();
    void IRecipient<RemovedGamesMessage>.Receive(RemovedGamesMessage message) => RefreshProgressionPageEnabled();

    #endregion

    #region Event Handlers

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Refresh if the progression page should be enabled
        RefreshProgressionPageEnabled();

        // TODO: Find better way of defining the page popups where the data context and other things inherit correctly
        // Set the data context for each popup menu item
        foreach (BasePage page in PageTabControl.Items.
                     // Get all tab items
                     OfType<System.Windows.Controls.TabItem>().
                     // Get the content of the tab items
                     Select(x => x.Content).
                     // Only get base pages
                     OfType<BasePage>())
        {
            // Set the data context
            if (page.PopupMenu is FrameworkElement f)
                f.DataContext = page.DataContext;
        }
    }

    #endregion
}