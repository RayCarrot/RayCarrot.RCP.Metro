namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GamePlatformInfoAttribute : Attribute
{
    // TODO-UPDATE: Localize
    public GamePlatformInfoAttribute(string displayName, GamePlatformIconAsset icon, bool requiresEmulator)
    {
        Icon = icon;
        RequiresEmulator = requiresEmulator;
        DisplayName = new ConstLocString(displayName);
    }

    public LocalizedString DisplayName { get; }
    public GamePlatformIconAsset Icon { get; }
    public bool RequiresEmulator { get; }
}