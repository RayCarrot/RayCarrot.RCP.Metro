namespace RayCarrot.RCP.Metro;

public abstract class GameOptionsDialog_EmulatorConfigPageViewModel : GameOptionsDialog_BasePageViewModel
{
    protected GameOptionsDialog_EmulatorConfigPageViewModel(LocalizedString pageName) : base(pageName, GenericIconKind.GameOptions_Emulator) { }

    /// <summary>
    /// Indicates if the page can be saved
    /// </summary>
    public override bool CanSave => true;
}