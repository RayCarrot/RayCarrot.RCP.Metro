namespace RayCarrot.RCP.Metro;

public record RemovedGamesMessage
{
    public RemovedGamesMessage(params GameInstallation[] gameInstallations) : this((IList<GameInstallation>)gameInstallations) { }

    public RemovedGamesMessage(IList<GameInstallation> gameInstallations)
    {
        GameInstallations = gameInstallations;

        Logger.Trace("Created a {0} with {1} removed games", nameof(RemovedGamesMessage), gameInstallations.Count);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IList<GameInstallation> GameInstallations { get; }
}