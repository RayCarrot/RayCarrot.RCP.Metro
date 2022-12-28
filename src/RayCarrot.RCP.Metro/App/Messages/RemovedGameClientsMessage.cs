using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public record RemovedGameClientsMessage(IList<GameClientInstallation> GameClientInstallations)
{
    public RemovedGameClientsMessage(params GameClientInstallation[] gameClientInstallations) 
        : this((IList<GameClientInstallation>)gameClientInstallations) { }
};