using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public record AddedGameClientsMessage(IList<GameClientInstallation> GameClientInstallations)
{
    public AddedGameClientsMessage(params GameClientInstallation[] gameClientInstallations) 
        : this((IList<GameClientInstallation>)gameClientInstallations) { }
};