namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class EmulatorVersionViewModel : BaseViewModel
{
    public EmulatorVersionViewModel(EmulatorVersion emulatorVersion)
    {
        EmulatorVersion = emulatorVersion;
        DisplayName = emulatorVersion.DisplayName;
    }

    public EmulatorVersion EmulatorVersion { get; }
    public LocalizedString DisplayName { get; }
}