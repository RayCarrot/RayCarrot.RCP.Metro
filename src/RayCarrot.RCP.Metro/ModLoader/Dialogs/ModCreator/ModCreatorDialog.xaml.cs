namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModCreator;

/// <summary>
/// Interaction logic for ModCreatorDialog.xaml
/// </summary>
public partial class ModCreatorDialog : WindowContentControl
{
    #region Constructor
    
    public ModCreatorDialog(ModCreatorViewModel viewModel)
    {
        DataContext = viewModel;

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

        WindowInstance.Title = Metro.Resources.ModCreator_Title;
        WindowInstance.Icon = GenericIconKind.Window_ModCreator;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 700;
        WindowInstance.Height = 700;
    }

    #endregion
}