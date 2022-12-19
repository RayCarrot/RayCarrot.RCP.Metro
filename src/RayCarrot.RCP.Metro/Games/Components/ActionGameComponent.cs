using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public abstract class ActionGameComponent : DescriptorComponent
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

    public Task InvokeAsync(GameInstallation gameInstallation) => _action(gameInstallation);
}