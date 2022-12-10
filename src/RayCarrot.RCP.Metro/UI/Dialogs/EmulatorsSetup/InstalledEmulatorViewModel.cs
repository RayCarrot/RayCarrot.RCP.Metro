using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

public class InstalledEmulatorViewModel : BaseViewModel
{
    public InstalledEmulatorViewModel(EmulatorInstallation emulatorInstallation)
    {
        EmulatorInstallation = emulatorInstallation;
        Platforms = new ObservableCollection<PlatformViewModel>(Descriptor.SupportedPlatforms.Select(x => new PlatformViewModel(x)));

        RemoveEmulatorCommand = new RelayCommand(RemoveEmulator);
    }

    public ICommand RemoveEmulatorCommand { get; }

    public EmulatorInstallation EmulatorInstallation { get; }
    public EmulatorDescriptor Descriptor => EmulatorInstallation.EmulatorDescriptor;
    public LocalizedString DisplayName => Descriptor.DisplayName;
    public EmulatorIconAsset Icon => Descriptor.Icon;

    public ObservableCollection<PlatformViewModel> Platforms { get; }

    public void RemoveEmulator()
    {
        Services.Emulators.RemoveEmulator(EmulatorInstallation);
    }

    // TODO: Add info
    // TODO: Add emulator specific options
}