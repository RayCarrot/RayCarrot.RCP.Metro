namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
public class Ray1ConfigFileNameComponent : FactoryGameComponent<string>
{
    public Ray1ConfigFileNameComponent(Func<GameInstallation, string> objFactory) : base(objFactory)
    { }
}