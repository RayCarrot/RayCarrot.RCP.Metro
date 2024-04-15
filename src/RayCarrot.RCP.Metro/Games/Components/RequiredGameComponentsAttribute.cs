namespace RayCarrot.RCP.Metro.Games.Components;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class RequiredGameComponentsAttribute : Attribute
{
    public RequiredGameComponentsAttribute(params Type[] requiredComponents)
    {
        RequiredComponents = requiredComponents;
    }

    /// <summary>
    /// Components which are required for this one to be used. Ideally components should
    /// be as self-contained as possible, but sometimes they require additional functionality
    /// provided by other components.
    /// </summary>
    public Type[] RequiredComponents { get; }
}