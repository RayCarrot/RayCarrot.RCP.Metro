namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponent(IsBase = true)]
public class OnGameLaunchedComponent : ActionGameComponent
{
    public OnGameLaunchedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameLaunchedComponent(Action<GameInstallation> action) : base(action) { }
}