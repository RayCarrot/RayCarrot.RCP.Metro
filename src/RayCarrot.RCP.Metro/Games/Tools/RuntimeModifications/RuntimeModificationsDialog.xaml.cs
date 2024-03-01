using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

/// <summary>
/// Interaction logic for RuntimeModificationsDialog.xaml
/// </summary>
public partial class RuntimeModificationsDialog : WindowContentControl
{
    #region Constructor

    public RuntimeModificationsDialog(RuntimeModificationsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        InitializeComponent();
    }

    #endregion

    #region Public Properties

    public RuntimeModificationsViewModel ViewModel { get; }
    public override bool IsResizable => true;

    #endregion

    #region Event Handlers

    private void UserControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ((FrameworkElement)sender).Focus();
    }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Runtime Modifications"; // TODO-LOC
        WindowInstance.Icon = GenericIconKind.Window_RuntimeModifications;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 700;
        WindowInstance.Height = 700;
    }

    #endregion

    #region Public Methods

    public override void Dispose()
    {
        base.Dispose();
        ViewModel.Dispose();
    }

    #endregion
}