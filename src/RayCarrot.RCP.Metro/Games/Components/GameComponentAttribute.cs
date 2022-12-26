namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GameComponentAttribute : Attribute
{
    public GameComponentAttribute(params Type[] requiredComponents)
    {
        RequiredComponents = requiredComponents;
    }

    // TODO-14: Another idea is to not have a base type for components, but rather every type they inherit from can
    //          be treated as a base type.
    /// <summary>
    /// Indicates if this is the base type for the game component. Each component has a base
    /// type which is how it gets referenced to from the provider. Think of it like a general
    /// interface/abstract class which can have multiple implementations.
    /// </summary>
    public bool IsBase { get; set; }

    /// <summary>
    /// If true then only a single instance of this component can be registered. The last
    /// one registered will be the last one used, thus overwriting existing ones.
    /// </summary>
    public bool SingleInstance { get; set; }

    /// <summary>
    /// Components which are required for this one to be used. Ideally components should
    /// be as self-contained as possible, but sometimes they require additional functionality
    /// provided by other components.
    /// </summary>
    public Type[] RequiredComponents { get; set; }
}