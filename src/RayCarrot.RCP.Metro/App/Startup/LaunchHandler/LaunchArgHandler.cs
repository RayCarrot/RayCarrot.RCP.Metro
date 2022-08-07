namespace RayCarrot.RCP.Metro;

public abstract class LaunchArgHandler
{
    public abstract bool DisableFullStartup { get; }

    public enum State
    {
        Startup,
        Running,
    }
}