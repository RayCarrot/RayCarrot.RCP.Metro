namespace RayCarrot.RCP.Metro;

public class GameOptionsDialog_OptionsPageViewModel : GameOptionsDialog_BasePageViewModel
{
    #region Constructor

    public GameOptionsDialog_OptionsPageViewModel(GameInstallation gameInstallation) 
        : base(new ResourceLocString(nameof(Resources.GameOptions_Options)), GenericIconKind.GameOptions_General)
    {
        // Set properties
        GameInstallation = gameInstallation;
        OptionsContent = gameInstallation.GameDescriptor.GetOptionsUI(gameInstallation);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The game options content
    /// </summary>
    public object? OptionsContent { get; }

    #endregion

    #region Protected Methods

    protected override object GetPageUI() => new GameOptions_Control()
    {
        DataContext = this
    };

    #endregion
}