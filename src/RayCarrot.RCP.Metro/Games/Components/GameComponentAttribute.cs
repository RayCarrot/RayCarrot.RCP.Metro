namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the base type for a game component to be registered, along with additional options
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GameComponentAttribute : Attribute
{
    /// <summary>
    /// If true then only a single instance of this component can be registered. The last
    /// one registered will be the last one used, thus overwriting existing ones.
    /// </summary>
    public bool SingleInstance { get; set; }
}