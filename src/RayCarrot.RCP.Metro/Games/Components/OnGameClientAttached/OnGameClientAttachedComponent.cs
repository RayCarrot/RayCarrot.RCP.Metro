using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class OnGameClientAttachedComponent : AsyncActionGameComponent<GameClientInstallation>
{
    public OnGameClientAttachedComponent(Func<GameInstallation, GameClientInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameClientAttachedComponent(Action<GameInstallation, GameClientInstallation> action) : base(action) { }
}