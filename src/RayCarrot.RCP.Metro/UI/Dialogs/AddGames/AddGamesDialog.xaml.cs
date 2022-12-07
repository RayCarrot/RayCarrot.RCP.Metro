namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for AddGamesDialog.xaml
/// </summary>
public partial class AddGamesDialog : WindowContentControl
{
    #region Constructor

    public AddGamesDialog()
    {
        // Set up UI
        InitializeComponent();

        // Set the data context
        DataContext = new AddGamesViewModel();
    }

    #endregion

    #region Public Properties

    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = "Add Games"; // TODO-UPDATE: Localize
        WindowInstance.Icon = GenericIconKind.Games;
        WindowInstance.MinWidth = 500;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 600;
        WindowInstance.Height = 600;
    }

    #endregion
}