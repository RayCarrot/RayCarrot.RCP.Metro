namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GameInfoAttribute : Attribute
{
    // TODO-UPDATE: Localize
    public GameInfoAttribute(string displayName, GameIconAsset gameIcon)
    {
        DisplayName = new ConstLocString(displayName);
        GameIcon = gameIcon;
    }

    public LocalizedString DisplayName { get; }
    public GameIconAsset GameIcon { get; }
}