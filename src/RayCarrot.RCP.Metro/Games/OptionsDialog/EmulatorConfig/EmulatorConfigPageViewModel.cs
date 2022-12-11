namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public abstract class EmulatorConfigPageViewModel : GameOptionsDialogPageViewModel
{
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_Emulator;

    /// <summary>
    /// Indicates if the page can be saved
    /// </summary>
    public override bool CanSave => true;
}