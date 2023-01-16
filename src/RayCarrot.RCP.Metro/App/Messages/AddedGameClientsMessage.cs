using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public record AddedGameClientsMessage
{
    public AddedGameClientsMessage(params GameClientInstallation[] gameClientInstallations) 
        : this((IList<GameClientInstallation>)gameClientInstallations) { }

    public AddedGameClientsMessage(IList<GameClientInstallation> gameClientInstallations)
    {
        GameClientInstallations = gameClientInstallations;
        Logger.Trace("Created a {0} with {1} added game clients", nameof(AddedGameClientsMessage), gameClientInstallations.Count);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IList<GameClientInstallation> GameClientInstallations { get; }
}