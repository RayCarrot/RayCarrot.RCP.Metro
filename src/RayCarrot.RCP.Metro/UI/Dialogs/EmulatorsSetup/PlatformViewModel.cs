namespace RayCarrot.RCP.Metro;

public class PlatformViewModel : BaseViewModel
{
    public PlatformViewModel(GamePlatform platform)
    {
        GamePlatformInfoAttribute info = platform.GetInfo();
        DisplayName = info.DisplayName;
        Icon = info.Icon;
    }

    public PlatformViewModel(LocalizedString displayName, GamePlatformIconAsset icon)
    {
        DisplayName = displayName;
        Icon = icon;
    }

    public LocalizedString DisplayName { get; }
    public GamePlatformIconAsset Icon { get; }
}