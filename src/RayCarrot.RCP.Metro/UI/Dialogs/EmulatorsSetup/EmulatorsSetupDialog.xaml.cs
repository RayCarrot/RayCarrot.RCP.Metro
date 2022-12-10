namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for EmulatorsSetupDialog.xaml
/// </summary>
public partial class EmulatorsSetupDialog : WindowContentControl
{
    #region Constructor

    public EmulatorsSetupDialog()
    {
        // Set up UI
        InitializeComponent();

        // Set the data context
        DataContext = new EmulatorsSetupViewModel();
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Configure emulators"; // TODO-UPDATE: Localize
        WindowInstance.Icon = GenericIconKind.Window_EmulatorsSetup;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 800;
        WindowInstance.Height = 600;
    }

    #endregion
}