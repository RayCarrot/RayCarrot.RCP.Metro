using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class GameClientGameConfigPageViewModel : GameOptionsDialogPageViewModel
{
    protected GameClientGameConfigPageViewModel(GameClientInstallation gameClientInstallation)
    {
        GameClientInstallation = gameClientInstallation;
    }

    public GameClientInstallation GameClientInstallation { get; }

    public override LocalizedString PageName => GameClientInstallation.GameClientDescriptor.DisplayName;
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_GameClient;

    /// <summary>
    /// Indicates if the page can be saved
    /// </summary>
    public override bool CanSave => true;
}