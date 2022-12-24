namespace RayCarrot.RCP.Metro.Games.Components;

public abstract class ActionGameComponent : GameComponent
{
    protected ActionGameComponent(Func<GameInstallation, Task> asyncAction)
    {
        _action = asyncAction;
    }
    protected ActionGameComponent(Action<GameInstallation> action)
    {
        _action = x =>
        {
            action(x);
            return Task.CompletedTask;
        };
    }

    private readonly Func<GameInstallation, Task> _action;

    public Task InvokeAsync() => _action(GameInstallation);
}