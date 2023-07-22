using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class GameClientsSetupViewModel : BaseViewModel, IInitializable,
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>, IRecipient<ModifiedGameClientsMessage>,
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>, IRecipient<ModifiedGamesMessage>,
    IRecipient<SortedGamesMessage>, IRecipient<SortedGameClientsMessage>
{
    public GameClientsSetupViewModel()
    {
        InstalledGameClients = new ObservableCollectionEx<InstalledGameClientViewModel>();
        AvailableGameClients = new ObservableCollection<AvailableGameClientViewModel>(
            Services.GameClients.GetGameClientDescriptors().Select(x => new AvailableGameClientViewModel(x)));

        Refresh();
    }

    public ObservableCollectionEx<InstalledGameClientViewModel> InstalledGameClients { get; }
    public ObservableCollection<AvailableGameClientViewModel> AvailableGameClients { get; }

    public InstalledGameClientViewModel? SelectedGameClient { get; set; }

    private void RefreshSupportedGames()
    {
        foreach (InstalledGameClientViewModel gameClient in InstalledGameClients)
            gameClient.RefreshSupportedGames();
    }

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

    public void Initialize() => Services.Messenger.RegisterAll(this);
    public void Deinitialize() => Services.Messenger.UnregisterAll(this);

    public void Receive(AddedGameClientsMessage message) => 
        Refresh(message.GameClientInstallations.FirstOrDefault());
    public void Receive(RemovedGameClientsMessage message) => 
        Refresh(SelectedGameClient?.GameClientInstallation);

    public void Receive(ModifiedGameClientsMessage message)
    {
        foreach (InstalledGameClientViewModel gameClient in InstalledGameClients)
            gameClient.RefreshDisplayName();
    }

    public void Receive(AddedGamesMessage message) =>
        RefreshSupportedGames();
    public void Receive(RemovedGamesMessage message) =>
        RefreshSupportedGames();
    public void Receive(ModifiedGamesMessage message)
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
    public void Receive(SortedGamesMessage message) =>
        RefreshSupportedGames();
    public void Receive(SortedGameClientsMessage message)
    {
        InstalledGameClients.ModifyCollection(x =>
            x.Sort((x1, x2) => message.SortedCollection.
                IndexOf(x1.GameClientInstallation).
                CompareTo(message.SortedCollection.
                    IndexOf(x2.GameClientInstallation))));
    }
}