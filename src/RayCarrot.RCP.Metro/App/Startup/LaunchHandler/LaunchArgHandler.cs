namespace RayCarrot.RCP.Metro;

public abstract class LaunchArgHandler
{
    public abstract LocalizedString DisplayName { get; }
    public abstract bool DisableFullStartup { get; }

    public enum State
    {
        Startup,
        Running,
    }
}