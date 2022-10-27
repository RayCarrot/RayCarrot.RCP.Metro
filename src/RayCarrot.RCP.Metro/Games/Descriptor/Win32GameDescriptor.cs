namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Win32 game
/// </summary>
public abstract class Win32GameDescriptor : GameDescriptor
{
    private Win32PlatformManager? _platformManager;

    public override GamePlatform Platform => GamePlatform.Win32;
    public override PlatformManager PlatformManager => _platformManager ??= new Win32PlatformManager(this);

    // TODO-14: Add abstract properties
}