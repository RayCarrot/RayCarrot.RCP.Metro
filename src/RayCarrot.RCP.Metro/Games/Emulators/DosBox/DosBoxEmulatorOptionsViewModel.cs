namespace RayCarrot.RCP.Metro.Games.Emulators.DosBox;

public class DosBoxEmulatorOptionsViewModel : EmulatorOptionsViewModel
{
    public DosBoxEmulatorOptionsViewModel(EmulatorInstallation emulatorInstallation) : base(emulatorInstallation) { }

    public FileSystemPath ConfigFilePath
    {
        get => EmulatorInstallation.GetValue(EmulatorDataKey.DosBox_ConfigFilePath, FileSystemPath.EmptyPath);
        set => EmulatorInstallation.SetValue(EmulatorDataKey.DosBox_ConfigFilePath, value);
    }
}