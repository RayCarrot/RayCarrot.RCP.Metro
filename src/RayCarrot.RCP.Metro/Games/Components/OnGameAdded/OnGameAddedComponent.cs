namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class OnGameAddedComponent : AsyncActionGameComponent
{
    public OnGameAddedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameAddedComponent(Action<GameInstallation> action) : base(action) { }
}