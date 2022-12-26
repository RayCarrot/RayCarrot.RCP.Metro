namespace RayCarrot.RCP.Metro;

/// <summary>
/// If used then only a single instance of this component can be registered. The last
/// one registered will be the last one used, thus overwriting existing ones.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SingleInstanceGameComponentAttribute : Attribute { }