namespace RayCarrot.RCP.Metro;

public abstract class ConfigureProgramInstallation<T>
    where T : ProgramInstallation
{
    protected ConfigureProgramInstallation(Action<T>? action)
    {
        _action = action;
    }
    protected ConfigureProgramInstallation(Func<T, Task> asyncAction)
    {
        _asyncAction = asyncAction;
    }

    private readonly Action<T>? _action;
    private readonly Func<T, Task>? _asyncAction;

    public Task Invoke(T programInstallation)
    {
        if (_action != null)
        {
            _action(programInstallation);
            return Task.CompletedTask;
        }
        else
        {
            return _asyncAction!(programInstallation);
        }
    }
}