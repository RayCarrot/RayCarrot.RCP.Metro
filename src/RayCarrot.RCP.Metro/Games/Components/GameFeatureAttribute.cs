namespace RayCarrot.RCP.Metro.Games.Components;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class GameFeatureAttribute : Attribute
{
    public GameFeatureAttribute(string displayNameResourceKey, GenericIconKind icon)
    {
        DisplayName = new ResourceLocString(displayNameResourceKey);
        Icon = icon;
    }

    public LocalizedString DisplayName { get; }
    public GenericIconKind Icon { get; }

}