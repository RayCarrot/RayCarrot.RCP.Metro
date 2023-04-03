using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.Steam;

namespace RayCarrot.RCP.Metro.Games.Finder;

public class SteamFinderQuery : FinderQuery
{
    public SteamFinderQuery(string steamId)
    {
        SteamId = steamId;
        ConfigureInstallation = ConfigureGameInstallationAsync;
    }

    private static async Task ConfigureGameInstallationAsync(ProgramInstallation programInstallation)
    {
        if (programInstallation is not GameInstallation gameInstallation) 
            return;

        // Find a Steam game client to attach by default
        GameClientInstallation? gameClientInstallation = Services.GameClients.GetInstalledGameClients().
            FirstOrDefault(x => x.GameClientDescriptor is SteamGameClientDescriptor && x.GameClientDescriptor.SupportsGame(gameInstallation, x));

        if (gameClientInstallation != null)
            await Services.GameClients.AttachGameClientAsync(gameInstallation, gameClientInstallation);
    }

    public string SteamId { get; }
}