namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class LocalGameLinksComponent : GameLinksComponent
{
    public LocalGameLinksComponent(Func<GameInstallation, IEnumerable<GameUriLink>> objFactory) : base(objFactory) { }
}