using System.Collections.Generic;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DescriptorComponentBuilder
{
    private readonly HashSet<DescriptorComponent> _components = new();

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <param name="component">The component to register</param>
    public void Register(DescriptorComponent component) => _components.Add(component);

    /// <summary>
    /// Builds the components to a provider
    /// </summary>
    /// <returns></returns>
    public DescriptorComponentProvider Build() => new(_components);
}