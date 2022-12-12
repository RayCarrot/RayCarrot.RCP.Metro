﻿using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

public class InstalledEmulatorViewModel : BaseViewModel
{
    public InstalledEmulatorViewModel(EmulatorInstallation emulatorInstallation)
    {
        EmulatorInstallation = emulatorInstallation;
        
        Platforms = new ObservableCollection<PlatformViewModel>(Descriptor.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        InfoItems = new ObservableCollection<DuoGridItemViewModel>(Descriptor.GetEmulatorInfoItems(emulatorInstallation));

        OptionsViewModel = Descriptor.GetEmulatorOptionsViewModel(emulatorInstallation);

        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
        RemoveEmulatorCommand = new AsyncRelayCommand(RemoveEmulatorAsync);
    }

    public ICommand OpenLocationCommand { get; }
    public ICommand RemoveEmulatorCommand { get; }

    public EmulatorInstallation EmulatorInstallation { get; }
    public EmulatorDescriptor Descriptor => EmulatorInstallation.EmulatorDescriptor;
    public LocalizedString DisplayName => Descriptor.DisplayName;
    public EmulatorIconAsset Icon => Descriptor.Icon;

    public ObservableCollection<PlatformViewModel> Platforms { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }

    public EmulatorOptionsViewModel? OptionsViewModel { get; }

    public Task OpenLocationAsync() => Services.File.OpenExplorerLocationAsync(EmulatorInstallation.InstallLocation);
    public Task RemoveEmulatorAsync() => Services.Emulators.RemoveEmulatorAsync(EmulatorInstallation);
}