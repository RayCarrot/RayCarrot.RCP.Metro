namespace RayCarrot.RCP.Metro;

public record ModifiedGamesMessage(IList<GameInstallation> GameInstallations, bool RebuiltComponents = false)
{
    public ModifiedGamesMessage(params GameInstallation[] gameInstallations) 
        : this((IList<GameInstallation>)gameInstallations) { }
    public ModifiedGamesMessage(GameInstallation gameInstallation, bool rebuiltComponents = false) 
        : this(new[] { gameInstallation }, rebuiltComponents) { }
};