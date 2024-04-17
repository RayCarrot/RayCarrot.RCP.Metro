using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.Panels;

public class RuntimeModificationsGamePanelViewModel : GamePanelViewModel
{
    #region Constructor

    public RuntimeModificationsGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        OpenCommand = new AsyncRelayCommand(OpenAsync);
    }

    #endregion

    #region Commands

    public ICommand OpenCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_RuntimeModification;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.GameTool_RuntimeModifications));

    #endregion

    #region Public Methods

    public async Task OpenAsync()
    {
        await Services.UI.ShowRuntimeModificationsAsync(GameInstallation);
    }

    #endregion
}