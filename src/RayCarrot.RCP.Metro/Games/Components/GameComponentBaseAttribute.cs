namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Indicates if this is the base type for the game component. Each component has a base
/// type which is how it gets referenced to from the provider. Think of it like a general
/// interface/abstract class which can have multiple implementations.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GameComponentBaseAttribute : Attribute
{
    /// <summary>
    /// Indicates if only a single instance of the component base type can be registered. The
    /// last one registered will be the last one used, thus overwriting existing ones.
    /// </summary>
    public bool SingleInstance { get; set; }
}