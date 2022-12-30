namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class ExternalGameLinksComponent : GameLinksComponent
{
    public ExternalGameLinksComponent(Func<GameInstallation, IEnumerable<GameUriLink>> objFactory) : base(objFactory) { }
}