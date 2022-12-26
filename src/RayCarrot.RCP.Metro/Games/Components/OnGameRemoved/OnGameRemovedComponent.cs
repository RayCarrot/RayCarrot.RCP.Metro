namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class OnGameRemovedComponent : ActionGameComponent
{
    public OnGameRemovedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameRemovedComponent(Action<GameInstallation> action) : base(action) { }
}