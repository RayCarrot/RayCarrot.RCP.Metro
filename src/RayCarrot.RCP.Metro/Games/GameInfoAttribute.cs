namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GameInfoAttribute : Attribute
{
    public GameInfoAttribute(string resourceKey, GameIconAsset gameIcon)
    {
        DisplayName = new ResourceLocString(resourceKey);
        GameIcon = gameIcon;
    }

    public LocalizedString DisplayName { get; }
    public GameIconAsset GameIcon { get; }
}