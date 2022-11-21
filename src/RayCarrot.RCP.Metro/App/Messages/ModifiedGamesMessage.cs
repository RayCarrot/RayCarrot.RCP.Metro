using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

public record ModifiedGamesMessage(IList<GameInstallation> GameInstallations)
{
    public ModifiedGamesMessage(params GameInstallation[] gameInstallations) : this((IList<GameInstallation>)gameInstallations) { }
};