namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModCreator;

/// <summary>
/// Interaction logic for ModCreatorDialog.xaml
/// </summary>
public partial class ModCreatorDialog : WindowContentControl
{
    #region Constructor
    
    public ModCreatorDialog()
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

        WindowInstance.Title = "Mod Creator"; // TODO-UPDATE: Localize
        WindowInstance.Icon = GenericIconKind.Window_ModCreator;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 900;
        WindowInstance.Height = 600;
    }

    #endregion
}