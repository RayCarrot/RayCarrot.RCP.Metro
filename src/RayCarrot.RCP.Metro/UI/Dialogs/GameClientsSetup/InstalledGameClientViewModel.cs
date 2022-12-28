using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class InstalledGameClientViewModel : BaseViewModel
{
    public InstalledGameClientViewModel(GameClientInstallation gameClientInstallation)
    {
        GameClientInstallation = gameClientInstallation;
        
        Platforms = new ObservableCollection<PlatformViewModel>(Descriptor.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        InfoItems = new ObservableCollection<DuoGridItemViewModel>(Descriptor.GetGameClientInfoItems(gameClientInstallation));

        OptionsViewModel = Descriptor.GetGameClientOptionsViewModel(gameClientInstallation);

        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
        RemoveGameClientCommand = new AsyncRelayCommand(RemoveGameClientAsync);
    }

    public ICommand OpenLocationCommand { get; }
    public ICommand RemoveGameClientCommand { get; }

    public GameClientInstallation GameClientInstallation { get; }
    public GameClientDescriptor Descriptor => GameClientInstallation.GameClientDescriptor;
    public LocalizedString DisplayName => Descriptor.DisplayName;
    public GameClientIconAsset Icon => Descriptor.Icon;

    public ObservableCollection<PlatformViewModel> Platforms { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }

    public GameClientOptionsViewModel? OptionsViewModel { get; }

    public Task OpenLocationAsync() => Services.File.OpenExplorerLocationAsync(GameClientInstallation.InstallLocation);
    public Task RemoveGameClientAsync() => Services.GameClients.RemoveGameClientAsync(GameClientInstallation);
}