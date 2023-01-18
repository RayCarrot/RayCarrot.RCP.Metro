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
        RenameCommand = new AsyncRelayCommand(RenameAsync);
        RemoveGameClientCommand = new AsyncRelayCommand(RemoveGameClientAsync);

        RefreshSupportedGames();
        RefreshDisplayName();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ICommand OpenLocationCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand RemoveGameClientCommand { get; }

    public GameClientInstallation GameClientInstallation { get; }
    public GameClientDescriptor Descriptor => GameClientInstallation.GameClientDescriptor;
    public LocalizedString DisplayName { get; private set; }
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

    [MemberNotNull(nameof(DisplayName))]
    public void RefreshDisplayName()
    {
        DisplayName = GameClientInstallation.GetDisplayName();
    }

    public Task OpenLocationAsync() => Services.File.OpenExplorerLocationAsync(GameClientInstallation.InstallLocation);

    public async Task RenameAsync()
    {
        StringInputResult result = await Services.UI.GetStringInputAsync(new StringInputViewModel
        {
            Title = "Rename game client/emulator", // TODO-UPDATE: Localize
            HeaderText = $"Rename {Descriptor.DisplayName}. Keep the name empty to use the default one.", // TODO-UPDATE: Localize
            StringInput = GameClientInstallation.GetValue<string>(GameClientDataKey.RCP_CustomName),
        });

        if (result.CanceledByUser)
            return;

        string? name = result.StringInput;

        if (name.IsNullOrWhiteSpace())
            name = null;

        GameClientInstallation.SetValue(GameClientDataKey.RCP_CustomName, name);

        Logger.Info("Renamed the game client {0} to {1}", GameClientInstallation.FullId, name);

        Services.Messenger.Send(new ModifiedGameClientsMessage(GameClientInstallation));
    }

    public Task RemoveGameClientAsync() => Services.GameClients.RemoveGameClientAsync(GameClientInstallation);
}