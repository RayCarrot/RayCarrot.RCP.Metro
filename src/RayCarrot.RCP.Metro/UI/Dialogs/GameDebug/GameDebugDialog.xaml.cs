using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameDebugDialog.xaml
/// </summary>
public partial class GameDebugDialog : WindowContentControl
{
    #region Constructor

    public GameDebugDialog(GameInstallation gameInstallation)
    {
        // Set up UI
        InitializeComponent();

        // Set the data context
        DataContext = new GameDebugViewModel(gameInstallation);
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Game debug";
        WindowInstance.Icon = GenericIconKind.Window_GameDebug;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 800;
        WindowInstance.Height = 600;
    }

    #endregion

    #region Event Handlers

    private void JsonViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling
        MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = e.Source
        };

        ContentScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }

    #endregion
}