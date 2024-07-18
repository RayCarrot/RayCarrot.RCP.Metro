using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.UbisoftConnect;

namespace RayCarrot.RCP.Metro.Games.Finder;

public class UbisoftConnectFinderQuery : FinderQuery
{
    public UbisoftConnectFinderQuery(string ubisoftConnectGameId)
    {
        UbisoftConnectGameId = ubisoftConnectGameId;
        ConfigureInstallation = ConfigureGameInstallationAsync;
    }

    private static async Task ConfigureGameInstallationAsync(ProgramInstallation programInstallation)
    {
        if (programInstallation is not GameInstallation gameInstallation) 
            return;

        // Find a Ubisoft Connect game client to attach by default
        GameClientInstallation? gameClientInstallation = Services.GameClients.GetInstalledGameClients().
            FirstOrDefault(x => x.GameClientDescriptor is UbisoftConnectGameClientDescriptor && x.GameClientDescriptor.SupportsGame(gameInstallation, x));

        if (gameClientInstallation != null)
            await Services.GameClients.AttachGameClientAsync(gameInstallation, gameClientInstallation);
    }

    public string UbisoftConnectGameId { get; }
}