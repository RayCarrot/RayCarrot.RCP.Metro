namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GamePlatformInfoAttribute : Attribute
{
    // TODO-UPDATE: Localize
    public GamePlatformInfoAttribute(string displayName, GamePlatformIconAsset icon)
    {
        Icon = icon;
        DisplayName = new ConstLocString(displayName);
    }

    public LocalizedString DisplayName { get; }
    public GamePlatformIconAsset Icon { get; }
}