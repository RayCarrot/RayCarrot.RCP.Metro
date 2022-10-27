namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a MS-DOS program
/// </summary>
public abstract class MSDOSGameDescriptor : GameDescriptor
{
    private MSDOSPlatformManager? _platformManager;

    public override GamePlatform Platform => GamePlatform.MSDOS;
    public override PlatformManager PlatformManager => _platformManager ??= new MSDOSPlatformManager(this);

    // TODO-14: Add abstract properties
}