namespace RayCarrot.RCP.Metro.Patcher.Dialogs.PatchCreator;

/// <summary>
/// Interaction logic for PatchCreatorDialog.xaml
/// </summary>
public partial class PatchCreatorDialog : WindowContentControl
{
    #region Constructor
    
    public PatchCreatorDialog()
    {
        // Set up UI
        InitializeComponent();
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = Metro.Resources.PatchCreator_Title;
        WindowInstance.Icon = GenericIconKind.Window_PatchCreator;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 900;
        WindowInstance.Height = 600;
    }

    #endregion
}