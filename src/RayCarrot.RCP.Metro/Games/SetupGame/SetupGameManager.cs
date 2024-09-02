namespace RayCarrot.RCP.Metro.Games.SetupGame;

public abstract class SetupGameManager
{
    protected SetupGameManager(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }

    public virtual IEnumerable<SetupGameAction> GetRecommendedActions() => Enumerable.Empty<SetupGameAction>();
    public virtual IEnumerable<SetupGameAction> GetOptionalActions() => Enumerable.Empty<SetupGameAction>();
    public virtual IEnumerable<SetupGameAction> GetIssueActions() => Enumerable.Empty<SetupGameAction>();
}