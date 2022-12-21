namespace RayCarrot.RCP.Metro.Games.Components;

public class DescriptorComponentProvider
{
    public DescriptorComponentProvider(IEnumerable<(Type Type, DescriptorComponent Component, ComponentPriority Priority)> components)
    {
        _components = components.
            GroupBy(x => x.Type).
            ToDictionary(x => x.Key, x => x.
                OrderByDescending(c => c.Priority).
                Select(c => c.Component).
                ToList());
    }

    private readonly Dictionary<Type, List<DescriptorComponent>> _components;

    public bool HasComponent<T>()
        where T : DescriptorComponent
    {
        return _components.ContainsKey(typeof(T));
    }

    public T? GetComponent<T>()
        where T : DescriptorComponent
    {
        return _components.TryGetValue(typeof(T), out List<DescriptorComponent> c) ? (T)c.First() : null;
    }

    public T GetRequiredComponent<T>()
        where T : DescriptorComponent
    {
        return GetComponent<T>() ?? throw new InvalidOperationException($"Component of type {typeof(T)} was not found");
    }

    public IEnumerable<T> GetComponents<T>()
        where T : DescriptorComponent
    {
        return _components.TryGetValue(typeof(T), out List<DescriptorComponent> c) ? c.Cast<T>() : Array.Empty<T>();
    }

    public IEnumerable<DescriptorComponent> GetComponents() => _components.Values.SelectMany(x => x);
}