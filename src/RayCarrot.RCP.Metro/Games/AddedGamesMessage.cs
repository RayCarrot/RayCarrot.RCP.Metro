namespace RayCarrot.RCP.Metro;

public record AddedGamesMessage
{
    public AddedGamesMessage(params GameInstallation[] gameInstallations) : this((IList<GameInstallation>)gameInstallations) { }

    public AddedGamesMessage(IList<GameInstallation> gameInstallations)
    {
        GameInstallations = gameInstallations;
        Logger.Trace("Created a {0} with {1} added games", nameof(AddedGamesMessage), gameInstallations.Count);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IList<GameInstallation> GameInstallations { get; }
}