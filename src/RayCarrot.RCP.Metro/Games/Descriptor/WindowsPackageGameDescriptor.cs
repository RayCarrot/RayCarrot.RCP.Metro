namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Windows package
/// </summary>
public abstract class WindowsPackageGameDescriptor : GameDescriptor
{
    private WindowsPackagePlatformManager? _platformManager;

    public override GamePlatform Platform => GamePlatform.WindowsPackage;
    public override PlatformManager PlatformManager => _platformManager ??= new WindowsPackagePlatformManager(this);

    // TODO-14: Add abstract properties
    //public abstract string PackageName { get; }
    //public abstract string FullPackageName { get; }
}