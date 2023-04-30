using System.Windows;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameClientsSetupDialog.xaml
/// </summary>
public partial class GameClientsSetupDialog : WindowContentControl
{
    #region Constructor

    public GameClientsSetupDialog()
    {
        // Set up UI
        InitializeComponent();

        // Set the data context
        DataContext = new GameClientsSetupViewModel();
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    public GameClientsSetupViewModel ViewModel => (GameClientsSetupViewModel)DataContext;

    #endregion

    #region Event Handlers

    private void InstalledGameClientsListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        DragDrop.SetDropHandler(InstalledGameClientsListBox, new GameClientsSetupDropHandler(ViewModel));
    }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = Metro.Resources.GameClients_ConfigTitle;
        WindowInstance.Icon = GenericIconKind.Window_GameClientsSetup;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 800;
        WindowInstance.Height = 600;
    }

    #endregion
}