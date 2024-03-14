namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class EmulatorViewModel : BaseViewModel
{
    public EmulatorViewModel(EmulatorManager emulatorManager)
    {
        EmulatorManager = emulatorManager;
        DisplayName = emulatorManager.DisplayName;
    }

    public EmulatorManager EmulatorManager { get; }
    public LocalizedString DisplayName { get; }
}