namespace RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;

/// <summary>
/// Interaction logic for PrototypeRestorationDialog.xaml
/// </summary>
public partial class PrototypeRestorationDialog : WindowContentControl
{
    #region Constructor

    public PrototypeRestorationDialog(PrototypeRestorationViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        InitializeComponent();
    }

    #endregion

    #region Public Properties

    public PrototypeRestorationViewModel ViewModel { get; }
    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = Metro.Resources.GameTool_PrototypeRestoration;
        WindowInstance.Icon = GenericIconKind.Window_PrototypeRestoration;
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