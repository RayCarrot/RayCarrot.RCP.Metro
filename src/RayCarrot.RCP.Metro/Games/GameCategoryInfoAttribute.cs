using System;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GameCategoryInfoAttribute : Attribute
{
    public GameCategoryInfoAttribute(string displayNameResourceKey, GenericIconKind icon)
    {
        DisplayName = new ResourceLocString(displayNameResourceKey);
        Icon = icon;
    }

    public LocalizedString DisplayName { get; }
    public GenericIconKind Icon { get; }
}