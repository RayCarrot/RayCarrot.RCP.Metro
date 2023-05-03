namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GameCategoryInfoAttribute : Attribute
{
    public GameCategoryInfoAttribute(string displayNameResourceKey, GameCategoryIconAsset icon)
    {
        DisplayName = new ResourceLocString(displayNameResourceKey);
        Icon = icon;
    }

    public LocalizedString DisplayName { get; }
    public GameCategoryIconAsset Icon { get; }
}