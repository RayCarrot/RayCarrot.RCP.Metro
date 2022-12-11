using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

public class AvailableEmulatorViewModel : BaseViewModel
{
    public AvailableEmulatorViewModel(EmulatorDescriptor descriptor)
    {
        Descriptor = descriptor;
        Platforms = new ObservableCollection<PlatformViewModel>(descriptor.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        
        AddEmulatorCommand = new AsyncRelayCommand(AddEmulatorAsync);
    }

    public ICommand AddEmulatorCommand { get; }

    public EmulatorDescriptor Descriptor { get; }
    public LocalizedString DisplayName => Descriptor.DisplayName;
    public EmulatorIconAsset Icon => Descriptor.Icon;

    public ObservableCollection<PlatformViewModel> Platforms { get; }

    public async Task AddEmulatorAsync()
    {
        FileBrowserResult broweResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select the emulator executable",
            ExtensionFilter = new FileExtension(".exe").GetFileFilterItem.StringRepresentation,
        });

        if (broweResult.CanceledByUser)
            return;

        Services.Emulators.AddEmulator(Descriptor, broweResult.SelectedFile);
    }
}