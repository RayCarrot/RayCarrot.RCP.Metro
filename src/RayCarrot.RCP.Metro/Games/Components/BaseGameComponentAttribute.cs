namespace RayCarrot.RCP.Metro;

/// <summary>
/// Indicates if this is the base type for the game component. Each component has a base
/// type which is how it gets referenced to from the provider. Think of it like a general
/// interface/abstract class which can have multiple implementations.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BaseGameComponentAttribute : Attribute
{
    // TODO-14: Another idea is to not have a base type for components, but rather
    //          every type they inherit from can be treated as a base type.
}