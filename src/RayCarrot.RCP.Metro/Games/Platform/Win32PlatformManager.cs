namespace RayCarrot.RCP.Metro;

public class Win32PlatformManager : PlatformManager
{
    public Win32PlatformManager(Win32GameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.MSDOS;
    public new Win32GameDescriptor GameDescriptor { get; }
}