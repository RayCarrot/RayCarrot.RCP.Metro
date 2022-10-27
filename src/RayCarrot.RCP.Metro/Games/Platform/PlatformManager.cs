namespace RayCarrot.RCP.Metro;

public abstract class PlatformManager
{
    protected PlatformManager(GameDescriptor gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public GameDescriptor GameDescriptor { get; }
    public abstract GamePlatform Platform { get; }
}