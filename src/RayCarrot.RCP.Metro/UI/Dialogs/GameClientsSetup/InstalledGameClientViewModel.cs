using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class InstalledGameClientViewModel : BaseViewModel
{
    public InstalledGameClientViewModel(GameClientInstallation gameClientInstallation)
    {
        GameClientInstallation = gameClientInstallation;
        
        // Only emulators define platforms
        if (Descriptor is EmulatorGameClientDescriptor emu)
            Platforms = new ObservableCollection<PlatformViewModel>(emu.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        else
            Platforms = new ObservableCollection<PlatformViewModel>();

        InfoItems = new ObservableCollection<DuoGridItemViewModel>(Descriptor.GetGameClientInfoItems(gameClientInstallation));

        OptionsViewModel = Descriptor.GetGameClientOptionsViewModel(gameClientInstallation);

        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
        RemoveGameClientCommand = new AsyncRelayCommand(RemoveGameClientAsync);

        RefreshSupportedGames();
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

    public ObservableCollection<GameInstallation> SupportedGames { get; set; }

    [MemberNotNull(nameof(SupportedGames))]
    public void RefreshSupportedGames()
    {
        SupportedGames = Services.Games.GetInstalledGames().
            Where(x => Descriptor.SupportsGame(x, GameClientInstallation)).
            ToObservableCollection();
    }

    public Task OpenLocationAsync() => Services.File.OpenExplorerLocationAsync(GameClientInstallation.InstallLocation);
    public Task RemoveGameClientAsync() => Services.GameClients.RemoveGameClientAsync(GameClientInstallation);
}