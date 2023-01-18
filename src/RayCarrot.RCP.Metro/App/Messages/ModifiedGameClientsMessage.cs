using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public record ModifiedGameClientsMessage
{
    public ModifiedGameClientsMessage(params GameClientInstallation[] gameClientInstallations) 
        : this((IList<GameClientInstallation>)gameClientInstallations) { }
    public ModifiedGameClientsMessage(IList<GameClientInstallation> gameClientInstallations)
    {
        GameClientInstallations = gameClientInstallations;
        Logger.Trace("Created a {0} with {1} modified game clients", nameof(ModifiedGameClientsMessage), gameClientInstallations.Count);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IList<GameClientInstallation> GameClientInstallations { get; }
}