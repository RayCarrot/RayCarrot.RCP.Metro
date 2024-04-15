using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[GameComponentBase(SingleInstance = true)]
public class LaunchArgumentsComponent : FactoryGameComponent<string?>
{
    public LaunchArgumentsComponent(Func<GameInstallation, string?> objFactory) : base(objFactory) { }
}