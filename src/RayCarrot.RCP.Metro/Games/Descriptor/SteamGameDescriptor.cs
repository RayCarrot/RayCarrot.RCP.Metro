namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Steam game
/// </summary>
public abstract class SteamGameDescriptor : GameDescriptor
{
    private SteamPlatformManager? _platformManager;

    public override GamePlatform Platform => GamePlatform.Steam;
    public override PlatformManager PlatformManager => _platformManager ??= new SteamPlatformManager(this);

    // TODO-14: Add abstract properties
}