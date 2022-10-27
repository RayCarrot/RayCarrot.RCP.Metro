namespace RayCarrot.RCP.Metro;

public class SteamPlatformManager : PlatformManager
{
    public SteamPlatformManager(SteamGameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.Steam;
    public new SteamGameDescriptor GameDescriptor { get; }
}