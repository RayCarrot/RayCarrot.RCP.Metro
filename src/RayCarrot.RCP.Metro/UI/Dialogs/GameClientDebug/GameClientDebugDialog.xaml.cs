using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameClientDebugDialog.xaml
/// </summary>
public partial class GameClientDebugDialog : WindowContentControl
{
    #region Constructor

    public GameClientDebugDialog(GameClientInstallation gameClientInstallation)
    {
        // Set up UI
        InitializeComponent();

        // Set the data context
        DataContext = new GameClientDebugViewModel(gameClientInstallation);
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Game client debug";
        WindowInstance.Icon = GenericIconKind.Window_GameClientDebug;
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