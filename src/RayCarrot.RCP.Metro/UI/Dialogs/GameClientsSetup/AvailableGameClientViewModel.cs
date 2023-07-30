using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro;

public class AvailableGameClientViewModel : BaseViewModel
    
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

        AddGameClientCommand = new AsyncRelayCommand(AddGameClientAsync);
        FindGameClientCommand = new AsyncRelayCommand(FindGameClientAsync);
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

    #region Public Methods

    public void Refresh()
    {
        // Only get the finder item if there are no added game clients with this descriptor
        FinderItem = !Services.GameClients.AnyInstalledGameClients(x => x.GameClientDescriptor == Descriptor)
            ? Descriptor.GetFinderItem()
            : null;
    }

    public async Task AddGameClientAsync()
    {
        InstallLocation location;

        if (Descriptor.InstallationRequiresFile)
        {
            FileBrowserResult broweResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
            {
                Title = Resources.GameClients_BrowseFileHeader,
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
                Title = Resources.GameClients_BrowseDirHeader,
            });

            if (broweResult.CanceledByUser)
                return;

            location = new InstallLocation(broweResult.SelectedDirectory);
        }

        // Make sure it's valid
        if (!Descriptor.IsValid(location))
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.GameClients_InvalidLocation, MessageType.Error);
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
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Finder_Error);
            return;
        }

        GameClientsManager.GameClientToAdd? gameClientToAdd = FinderItem.GetGameClientToAdd();

        if (gameClientToAdd != null)
        {
            // Add the found game client
            await Services.GameClients.AddGameClientAsync(gameClientToAdd);

            await Services.MessageUI.DisplayMessageAsync(
                String.Format(Resources.GameClients_FindSuccessResult, gameClientToAdd.InstallLocation), Resources.GameClients_FindResultHeader, MessageType.Success);
        }
        else
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.GameClients_FindFailedResult, Resources.GameClients_FindResultHeader, MessageType.Information);
        }
    }

    #endregion
}