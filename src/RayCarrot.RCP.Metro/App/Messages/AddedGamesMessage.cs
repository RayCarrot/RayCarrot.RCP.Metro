using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public record AddedGamesMessage(IList<GameInstallation> GameInstallations)
{
    public AddedGamesMessage(params GameInstallation[] gameInstallations) : this((IList<GameInstallation>)gameInstallations) { }
};