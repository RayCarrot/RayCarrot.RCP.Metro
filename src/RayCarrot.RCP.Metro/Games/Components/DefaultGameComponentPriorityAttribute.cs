using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class DefaultGameComponentPriorityAttribute : Attribute
{
    public DefaultGameComponentPriorityAttribute(ComponentPriority defaultPriority)
    {
        DefaultPriority = defaultPriority;
    }

    public ComponentPriority DefaultPriority { get; }
}