using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public record RemovedGameClientsMessage
{
    public RemovedGameClientsMessage(params GameClientInstallation[] gameClientInstallations) 
        : this((IList<GameClientInstallation>)gameClientInstallations) { }

    public RemovedGameClientsMessage(IList<GameClientInstallation> gameClientInstallations)
    {
        GameClientInstallations = gameClientInstallations;

        Logger.Trace("Created a {0} with {1} removed game clients", nameof(RemovedGameClientsMessage), gameClientInstallations.Count);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IList<GameClientInstallation> GameClientInstallations { get; }
}