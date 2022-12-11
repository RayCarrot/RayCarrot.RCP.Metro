namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class EmulatorConfigPageViewModel : GameOptionsDialogPageViewModel
{
    public override LocalizedString PageName => "Emulator"; // TODO-UPDATE: Localize
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_Emulator;

    /// <summary>
    /// Indicates if the page can be saved
    /// </summary>
    public override bool CanSave => true; // TODO-14: Depends on emulator...
}