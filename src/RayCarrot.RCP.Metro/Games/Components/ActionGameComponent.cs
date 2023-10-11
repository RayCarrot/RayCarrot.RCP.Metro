namespace RayCarrot.RCP.Metro.Games.Components;

public abstract class ActionGameComponent : GameComponent
{
    protected ActionGameComponent(Action<GameInstallation> action)
    {
        _action = action;
    }

    private readonly Action<GameInstallation> _action;

    public void Invoke() => _action(GameInstallation);
}

public abstract class ActionGameComponent<T> : GameComponent
{
    protected ActionGameComponent(Action<GameInstallation, T> action)
    {
        _action = action;
    }

    private readonly Action<GameInstallation, T> _action;

    public void Invoke(T arg) => _action(GameInstallation, arg);
}