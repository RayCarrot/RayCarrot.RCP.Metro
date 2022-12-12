namespace RayCarrot.RCP.Metro.Games.Emulators.DosBox;

public class DosBoxEmulatorOptionsViewModel : EmulatorOptionsViewModel
{
    public DosBoxEmulatorOptionsViewModel(EmulatorInstallation emulatorInstallation, EmulatorDescriptor emulatorDescriptor) 
        : base(emulatorInstallation)
    {
        EmulatorDescriptor = emulatorDescriptor;
    }

    public EmulatorDescriptor EmulatorDescriptor { get; }

    public FileSystemPath ConfigFilePath
    {
        get => EmulatorInstallation.GetValue(EmulatorDataKey.DosBox_ConfigFilePath, FileSystemPath.EmptyPath);
        set
        {
            EmulatorInstallation.SetValue(EmulatorDataKey.DosBox_ConfigFilePath, value);
            EmulatorDescriptor.RefreshEmulatedGames(EmulatorInstallation);
        }
    }
}