using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class AvailableGameClientViewModel : BaseViewModel
{
    public AvailableGameClientViewModel(GameClientDescriptor descriptor)
    {
        Descriptor = descriptor;

        // Only emulators define platforms
        if (descriptor is EmulatorGameClientDescriptor emu)
            Platforms = new ObservableCollection<PlatformViewModel>(emu.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        else
            Platforms = new ObservableCollection<PlatformViewModel>();
        
        AddGameClientCommand = new AsyncRelayCommand(AddGameClientAsync);
    }

    public ICommand AddGameClientCommand { get; }

    public GameClientDescriptor Descriptor { get; }
    public LocalizedString DisplayName => Descriptor.DisplayName;
    public GameClientIconAsset Icon => Descriptor.Icon;

    public ObservableCollection<PlatformViewModel> Platforms { get; }

    public async Task AddGameClientAsync()
    {
        FileBrowserResult broweResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select the game client executable",
            ExtensionFilter = new FileExtension(".exe").GetFileFilterItem.StringRepresentation,
        });

        if (broweResult.CanceledByUser)
            return;

        // Make sure it's valid
        if (!Descriptor.IsValid(broweResult.SelectedFile))
        {
            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The selected location is not valid", MessageType.Error);
            return;
        }

        await Services.GameClients.AddGameClientAsync(Descriptor, broweResult.SelectedFile);
    }
}