using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class OnGameClientAttachedComponent : ActionGameComponent<GameClientInstallation>
{
    public OnGameClientAttachedComponent(Func<GameInstallation, GameClientInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameClientAttachedComponent(Action<GameInstallation, GameClientInstallation> action) : base(action) { }
}