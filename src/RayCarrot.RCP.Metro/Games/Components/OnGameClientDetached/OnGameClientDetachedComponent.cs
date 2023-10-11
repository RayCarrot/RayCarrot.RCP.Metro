using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class OnGameClientDetachedComponent : AsyncActionGameComponent<GameClientInstallation>
{
    public OnGameClientDetachedComponent(Func<GameInstallation, GameClientInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameClientDetachedComponent(Action<GameInstallation, GameClientInstallation> action) : base(action) { }
}