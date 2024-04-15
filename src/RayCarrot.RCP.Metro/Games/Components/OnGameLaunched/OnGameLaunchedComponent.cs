namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class OnGameLaunchedComponent : AsyncActionGameComponent
{
    public OnGameLaunchedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameLaunchedComponent(Action<GameInstallation> action) : base(action) { }
}