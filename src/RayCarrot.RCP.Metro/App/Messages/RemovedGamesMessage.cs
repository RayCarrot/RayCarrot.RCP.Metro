using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public record RemovedGamesMessage(IList<GameInstallation> GameInstallations)
{
    public RemovedGamesMessage(params GameInstallation[] gameInstallations) : this((IList<GameInstallation>)gameInstallations) { }
};