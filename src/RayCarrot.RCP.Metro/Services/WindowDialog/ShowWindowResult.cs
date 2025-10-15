namespace RayCarrot.RCP.Metro;

public class ShowWindowResult
{
    public ShowWindowResult(bool success, IWindowControl? blockingWindowInstance)
    {
        Success = success;
        BlockingWindowInstance = blockingWindowInstance;
    }

    public bool Success { get; }
    public IWindowControl? BlockingWindowInstance { get; }
}