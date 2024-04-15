namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Optional attribute to use on game components. Unlike <see cref="GameComponentBaseAttribute"/>
/// this is not required on instances, but rather used to set additional properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class GameComponentInstanceAttribute : Attribute
{
    /// <summary>
    /// The default priority for registering the component
    /// </summary>
    public ComponentPriority DefaultPriority { get; set; }
}