namespace RayCarrot.RCP.Metro;

public record ModifiedGamesMessage
{
    public ModifiedGamesMessage(params GameInstallation[] gameInstallations) 
        : this((IList<GameInstallation>)gameInstallations) { }
    public ModifiedGamesMessage(GameInstallation gameInstallation, bool rebuiltComponents = false) 
        : this(new[] { gameInstallation }, rebuiltComponents) { }

    public ModifiedGamesMessage(IList<GameInstallation> gameInstallations, bool rebuiltComponents = false)
    {
        GameInstallations = gameInstallations;
        RebuiltComponents = rebuiltComponents;

        if (rebuiltComponents)
            Logger.Trace("Created a {0} with {1} modified games after having rebuilt components", nameof(ModifiedGamesMessage), gameInstallations.Count);
        else
            Logger.Trace("Created a {0} with {1} modified games", nameof(ModifiedGamesMessage), gameInstallations.Count);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IList<GameInstallation> GameInstallations { get; }
    public bool RebuiltComponents { get; }
}