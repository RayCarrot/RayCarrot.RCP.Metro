namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// The priority of a component. This is used when there are multiple components registered for the same type.
/// </summary>
public enum ComponentPriority
{
    Low = -1,
    Normal = 0,
    High = 1,
}