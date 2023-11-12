using System.Windows.Input;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro;

public class GameClientsSetupViewModel : BaseViewModel, IInitializable,
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>, IRecipient<ModifiedGameClientsMessage>,
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>,
    IRecipient<SortedGamesMessage>, IRecipient<SortedGameClientsMessage>
{
    #region Constructor

    public GameClientsSetupViewModel()
    {
        InstalledGameClients = new ObservableCollectionEx<InstalledGameClientViewModel>();
        AvailableGameClients = new ObservableCollection<AvailableGameClientViewModel>(
            Services.GameClients.GetGameClientDescriptors().Select(x => new AvailableGameClientViewModel(x)));

        Refresh();

        FindGameClientsCommand = new AsyncRelayCommand(FindGameClientsAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand FindGameClientsCommand { get; }

    #endregion

    #region Public Properties

    public ObservableCollectionEx<InstalledGameClientViewModel> InstalledGameClients { get; }
    public ObservableCollection<AvailableGameClientViewModel> AvailableGameClients { get; }

    public InstalledGameClientViewModel? SelectedGameClient { get; set; }

    public bool IsFinderRunning { get; set; }

    #endregion

    #region Private Methods

    private void RefreshSupportedGames()
    {
        foreach (InstalledGameClientViewModel gameClient in InstalledGameClients)
            gameClient.RefreshSupportedGames();
    }

    #endregion

    #region Public Methods

    public void Refresh(GameClientInstallation? selectedGameClientInstallation = null)
    {
        foreach (AvailableGameClientViewModel gameClient in AvailableGameClients)
            gameClient.Refresh();

        InstalledGameClients.ModifyCollection(x =>
        {
            x.Clear();

            foreach (GameClientInstallation gameClientInstallation in Services.GameClients.GetInstalledGameClients())
            {
                InstalledGameClientViewModel viewModel = new(gameClientInstallation);
                x.Add(viewModel);

                if (selectedGameClientInstallation == gameClientInstallation)
                    SelectedGameClient = viewModel;
            }
        });
    }

    public async Task FindGameClientsAsync()
    {
        if (IsFinderRunning)
            return;

        IsFinderRunning = true;

        FinderItem[] runFinderItems;

        try
        {
            // Create a finder
            Finder finder = new(Finder.DefaultOperations, Services.GameClients.GetFinderItems().ToArray());

            // Run the finder
            await finder.RunAsync();

            // Get the finder items
            runFinderItems = finder.FinderItems;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Running finder");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Finder_Error);
            return;
        }
        finally
        {
            IsFinderRunning = false;
        }

        // Get the game clients to add
        IEnumerable<GameClientsManager.GameClientToAdd> gameClientsToAdd = runFinderItems.
            OfType<GameClientFinderItem>().
            Select(x => x.GetGameClientToAdd()).
            Where(x => x != null)!;

        // Add the found game clients
        IList<GameClientInstallation> addedGameClients = await Services.GameClients.AddGameClientsAsync(gameClientsToAdd);

        if (addedGameClients.Any())
        {
            Logger.Info("The finder found {0} game clients", addedGameClients.Count);

            await Services.MessageUI.DisplayMessageAsync($"{Resources.Finder_FoundClients}{Environment.NewLine}{Environment.NewLine}• {addedGameClients.Select(x => x.GetDisplayName()).JoinItems(Environment.NewLine + "• ")}", Resources.Finder_FoundClientsHeader, MessageType.Success);
        }
        else
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.Finder_NoGameClientResults, Resources.Finder_ResultHeader, MessageType.Information);
        }
    }

    public void Initialize() => Services.Messenger.RegisterAll(this);
    public void Deinitialize() => Services.Messenger.UnregisterAll(this);

    #endregion

    #region Message Receivers

    void IRecipient<AddedGameClientsMessage>.Receive(AddedGameClientsMessage message) =>
        Refresh(message.GameClientInstallations.FirstOrDefault());
    void IRecipient<RemovedGameClientsMessage>.Receive(RemovedGameClientsMessage message) =>
        Refresh(SelectedGameClient?.GameClientInstallation);

    void IRecipient<ModifiedGameClientsMessage>.Receive(ModifiedGameClientsMessage message)
    {
        foreach (InstalledGameClientViewModel gameClient in InstalledGameClients)
            gameClient.RefreshDisplayName();
    }

    void IRecipient<AddedGamesMessage>.Receive(AddedGamesMessage message) =>
        RefreshSupportedGames();
    void IRecipient<RemovedGamesMessage>.Receive(RemovedGamesMessage message) =>
        RefreshSupportedGames();
    void IRecipient<ModifiedGamesMessage>.Receive(ModifiedGamesMessage message)
    {
        foreach (InstalledGameClientViewModel installedGameClient in InstalledGameClients)
        {
            foreach (SupportedGameViewModel supportedGame in installedGameClient.SupportedGames)
            {
                if (message.GameInstallations.Contains(supportedGame.GameInstallation))
                    supportedGame.RefreshUsesGameClient();
            }
        }
    }
    void IRecipient<SortedGamesMessage>.Receive(SortedGamesMessage message) =>
        RefreshSupportedGames();
    void IRecipient<SortedGameClientsMessage>.Receive(SortedGameClientsMessage message)
    {
        InstalledGameClients.ModifyCollection(x =>
            x.Sort((x1, x2) => message.SortedCollection.
                IndexOf(x1.GameClientInstallation).
                CompareTo(message.SortedCollection.
                    IndexOf(x2.GameClientInstallation))));
    }

    #endregion
}