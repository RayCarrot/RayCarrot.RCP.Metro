namespace RayCarrot.RCP.Metro.Games.Components;

public class DescriptorComponentBuilder
{
    private readonly HashSet<(Type Type, DescriptorComponent Component, ComponentPriority Priority)> _components = new();

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="component">The component to register</param>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public void Register<T>(T component, ComponentPriority priority = ComponentPriority.Normal) 
        where T : DescriptorComponent 
        => _components.Add((typeof(T), component, priority));

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public void Register<T>(ComponentPriority priority = ComponentPriority.Normal)
        where T : DescriptorComponent, new() 
        => _components.Add((typeof(T), new T(), priority));

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <typeparam name="U">The type of the component instance</typeparam>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public void Register<T, U>(ComponentPriority priority = ComponentPriority.Normal) 
        where T : DescriptorComponent 
        where U : T, new()
        => _components.Add((typeof(T), new U(), priority));

    /// <summary>
    /// Builds the components to a provider
    /// </summary>
    /// <returns></returns>
    public DescriptorComponentProvider Build() => new(_components);
}