namespace RayCarrot.RCP.Metro.Games.Emulators;

public abstract class EmulatorOptionsViewModel : BaseViewModel
{
    protected EmulatorOptionsViewModel(EmulatorInstallation emulatorInstallation)
    {
        EmulatorInstallation = emulatorInstallation;
    }

    public EmulatorInstallation EmulatorInstallation { get; }
}