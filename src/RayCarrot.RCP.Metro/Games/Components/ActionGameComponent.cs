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

public abstract class ActionGameComponent<T> : GameComponent
{
    protected ActionGameComponent(Func<GameInstallation, T, Task> asyncAction)
    {
        _action = asyncAction;
    }
    protected ActionGameComponent(Action<GameInstallation, T> action)
    {
        _action = (x, a) =>
        {
            action(x, a);
            return Task.CompletedTask;
        };
    }

    private readonly Func<GameInstallation, T, Task> _action;

    public Task InvokeAsync(T arg) => _action(GameInstallation, arg);
}