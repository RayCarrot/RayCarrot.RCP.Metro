namespace RayCarrot.RCP.Metro.Games.SetupGame;

public abstract class SetupGameAction
{ 
    public abstract LocalizedString Header { get; }
    public abstract LocalizedString Info { get; }

    public abstract SetupGameActionType Type { get; }

    public abstract GenericIconKind FixActionIcon { get; }
    public abstract LocalizedString? FixActionDisplayName { get; }

    public virtual bool CheckIsAvailable(GameInstallation gameInstallation) => true;

    public abstract bool CheckIsComplete(GameInstallation gameInstallation);

    public abstract Task FixAsync(GameInstallation gameInstallation);
}