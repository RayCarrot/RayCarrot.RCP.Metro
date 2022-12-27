using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[BaseGameComponent]
[SingleInstanceGameComponent]
public class LaunchArgumentsComponent : FactoryGameComponent<string?>
{
    public LaunchArgumentsComponent(Func<GameInstallation, string?> objFactory) : base(objFactory) { }
}