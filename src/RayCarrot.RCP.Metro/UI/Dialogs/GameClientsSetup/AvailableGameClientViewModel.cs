using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro;

public class AvailableGameClientViewModel : BaseViewModel, 
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>
{
    #region Constructor

    public AvailableGameClientViewModel(GameClientDescriptor descriptor)
    {
        Descriptor = descriptor;

        // Only emulators define platforms
        if (descriptor is EmulatorGameClientDescriptor emu)
            Platforms = new ObservableCollection<PlatformViewModel>(emu.SupportedPlatforms.Select(x => new PlatformViewModel(x)));
        else
            Platforms = new ObservableCollection<PlatformViewModel>();

        Refresh();

        AddGameClientCommand = new AsyncRelayCommand(AddGameClientAsync);
        FindGameClientCommand = new AsyncRelayCommand(FindGameClientAsync);

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand AddGameClientCommand { get; }
    public ICommand FindGameClientCommand { get; }

    #endregion

    #region Public Properties

    public GameClientDescriptor Descriptor { get; }
    public LocalizedString DisplayName => Descriptor.DisplayName;
    public GameClientIconAsset Icon => Descriptor.Icon;

    public GameClientFinderItem? FinderItem { get; set; }
    public ObservableCollection<PlatformViewModel> Platforms { get; }

    #endregion

    #region Private Methods

    private void Refresh()
    {
        // Only get the finder item if there are no added game clients with this descriptor
        FinderItem = !Services.GameClients.AnyInstalledGameClients(x => x.GameClientDescriptor == Descriptor)
            ? Descriptor.GetFinderItem()
            : null;
    }

    #endregion

    #region Public Methods

    public async Task AddGameClientAsync()
    {
        InstallLocation location;

        if (Descriptor.InstallationRequiresFile)
        {
            FileBrowserResult broweResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
            {
                // TODO-UPDATE: Localize
                Title = "Select the game client executable",
                ExtensionFilter = new FileExtension(".exe").GetFileFilterItem.StringRepresentation,
            });

            if (broweResult.CanceledByUser)
                return;

            location = InstallLocation.FromFilePath(broweResult.SelectedFile);
        }
        else
        {
            DirectoryBrowserResult broweResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
            {
                // TODO-UPDATE: Localize
                Title = "Select the game client installation",
            });

            if (broweResult.CanceledByUser)
                return;

            location = new InstallLocation(broweResult.SelectedDirectory);
        }

        // Make sure it's valid
        if (!Descriptor.IsValid(location))
        {
            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The selected location is not valid", MessageType.Error);
            return;
        }

        await Services.GameClients.AddGameClientAsync(Descriptor, location);
    }

    public async Task FindGameClientAsync()
    {
        if (FinderItem == null)
            return;

        try
        {
            // Create a finder
            Finder finder = new(Finder.DefaultOperations, FinderItem.YieldToArray<FinderItem>());

            // Run the finder
            finder.Run();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Running finder for a game client");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex,
                // TODO-UPDATE: Update localization
                Resources.GameFinder_Error);
            return;
        }

        if (FinderItem.HasBeenFound)
        {
            // Have to get the location here since FinderItem is null after the game client gets added
            InstallLocation foundLocation = FinderItem.FoundLocation.Value;

            // Add the found game client
            await Services.GameClients.AddGameClientAsync(FinderItem.GameClientDescriptor, foundLocation);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync($"The game client/emulator was found at {foundLocation}", "Finder result", MessageType.Success);
        }
        else
        {
            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The game client/emulator was not found", "Finder result", MessageType.Information);
        }
    }

    public void Receive(AddedGameClientsMessage message) => Refresh();
    public void Receive(RemovedGameClientsMessage message) => Refresh();

    #endregion
}