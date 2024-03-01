using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.Panels;

public class PrototypeRestorationGamePanelViewModel : GamePanelViewModel
{
    #region Constructor

    public PrototypeRestorationGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        OpenCommand = new AsyncRelayCommand(OpenAsync);
    }

    #endregion

    #region Commands

    public ICommand OpenCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_PrototypeRestoration;
    public override LocalizedString Header => "Prototype Restoration"; // TODO-LOC

    #endregion

    #region Public Methods

    public async Task OpenAsync()
    {
        await Services.UI.ShowPrototypeRestorationAsync(GameInstallation);
    }

    #endregion
}