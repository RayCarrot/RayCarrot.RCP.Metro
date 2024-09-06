namespace RayCarrot.RCP.Metro;

public record FixedSetupGameActionMessage
{
    public FixedSetupGameActionMessage(params GameInstallation[] gameInstallations)
        : this((IList<GameInstallation>)gameInstallations) { }
    public FixedSetupGameActionMessage(GameInstallation gameInstallation)
        : this(new[] { gameInstallation }) { }

    public FixedSetupGameActionMessage(IList<GameInstallation> gameInstallations)
    {
        GameInstallations = gameInstallations;
    }

    public IList<GameInstallation> GameInstallations { get; }
}