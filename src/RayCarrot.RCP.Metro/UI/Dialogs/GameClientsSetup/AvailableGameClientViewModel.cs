using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class AvailableGameClientViewModel : BaseViewModel
{
    public AvailableGameClientViewModel(GameClientDescriptor descriptor)
    {
        Descriptor = descriptor;
        Platforms = new ObservableCollection<PlatformViewModel>(descriptor.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        
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

        await Services.GameClients.AddGameClientAsync(Descriptor, broweResult.SelectedFile);
    }
}