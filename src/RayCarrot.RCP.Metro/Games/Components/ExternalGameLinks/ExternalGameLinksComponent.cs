namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class ExternalGameLinksComponent : GameLinksComponent
{
    public ExternalGameLinksComponent(Func<GameInstallation, IEnumerable<GameUriLink>> objFactory) : base(objFactory) { }
}