﻿using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class GameClientsSetupViewModel : BaseViewModel, 
    IRecipient<AddedGameClientsMessage>, IRecipient<RemovedGameClientsMessage>, 
    IRecipient<AddedGamesMessage>, IRecipient<RemovedGamesMessage>
{
    public GameClientsSetupViewModel()
    {
        InstalledGameClients = new ObservableCollection<InstalledGameClientViewModel>();
        AvailableGameClients = new ObservableCollection<AvailableGameClientViewModel>(
            Services.GameClients.GetGameCientDescriptors().Select(x => new AvailableGameClientViewModel(x)));

        Refresh();

        Services.Messenger.RegisterAll(this);
    }

    public ObservableCollection<InstalledGameClientViewModel> InstalledGameClients { get; }
    public ObservableCollection<AvailableGameClientViewModel> AvailableGameClients { get; }

    public InstalledGameClientViewModel? SelectedGameClient { get; set; }

    private void RefreshSupportedGames()
    {
        foreach (InstalledGameClientViewModel gameClient in InstalledGameClients)
            gameClient.RefreshSupportedGames();
    }

    public void Refresh(GameClientInstallation? selectedGameClientInstallation = null)
    {
        InstalledGameClients.Clear();

        foreach (GameClientInstallation gameClientInstallation in Services.GameClients.GetInstalledGameClients())
        {
            InstalledGameClientViewModel viewModel = new(gameClientInstallation);
            InstalledGameClients.Add(viewModel);

            if (selectedGameClientInstallation == gameClientInstallation)
                SelectedGameClient = viewModel;
        }
    }

    public void Receive(AddedGameClientsMessage message) => 
        Refresh(message.GameClientInstallations.FirstOrDefault());
    public void Receive(RemovedGameClientsMessage message) => 
        Refresh(SelectedGameClient?.GameClientInstallation);

    public void Receive(AddedGamesMessage message) =>
        RefreshSupportedGames();
    public void Receive(RemovedGamesMessage message) =>
        RefreshSupportedGames();
}