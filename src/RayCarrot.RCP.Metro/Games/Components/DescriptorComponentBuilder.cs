namespace RayCarrot.RCP.Metro.Games.Components;

public class DescriptorComponentBuilder
{
    private readonly HashSet<(Type Type, DescriptorComponent Component)> _components = new();

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="component">The component to register</param>
    public void Register<T>(T component) 
        where T : DescriptorComponent 
        => _components.Add((typeof(T), component));

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    public void Register<T>() 
        where T : DescriptorComponent, new() 
        => _components.Add((typeof(T), new T()));

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <typeparam name="U">The type of the component instance</typeparam>
    public void Register<T, U>() 
        where T : DescriptorComponent 
        where U : T, new()
        => _components.Add((typeof(T), new U()));

    /// <summary>
    /// Builds the components to a provider
    /// </summary>
    /// <returns></returns>
    public DescriptorComponentProvider Build() => new(_components);
}