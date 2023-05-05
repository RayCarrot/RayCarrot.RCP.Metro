namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GamePlatformInfoAttribute : Attribute
{
    public GamePlatformInfoAttribute(string resourceKey, GamePlatformIconAsset icon)
    {
        Icon = icon;
        DisplayName = new ResourceLocString(resourceKey);
    }

    public LocalizedString DisplayName { get; }
    public GamePlatformIconAsset Icon { get; }
}