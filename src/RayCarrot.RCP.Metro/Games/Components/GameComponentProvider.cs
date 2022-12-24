namespace RayCarrot.RCP.Metro.Games.Components;

public class GameComponentProvider
{
    public GameComponentProvider(IEnumerable<(Type Type, GameComponent Component)> components, GameInstallation gameInstallation)
    {
        _components = new Dictionary<Type, List<GameComponent>>();

        foreach ((Type Type, GameComponent Component) component in components)
        {
            // Initialize the component
            component.Component.Initialize(gameInstallation);

            // Add the component to a list
            if (!_components.TryGetValue(component.Type, out List<GameComponent> list))
            {
                list = new List<GameComponent>();
                _components[component.Type] = list;
            }

            list.Add(component.Component);
        }
    }

    private readonly Dictionary<Type, List<GameComponent>> _components;

    public bool HasComponent<T>()
        where T : GameComponent
    {
        return _components.ContainsKey(typeof(T));
    }

    public T? GetComponent<T>()
        where T : GameComponent
    {
        return _components.TryGetValue(typeof(T), out List<GameComponent> c) ? (T)c.First() : null;
    }

    public IEnumerable<T> GetComponents<T>()
        where T : GameComponent
    {
        return _components.TryGetValue(typeof(T), out List<GameComponent> c) ? c.Cast<T>() : Array.Empty<T>();
    }

    public IEnumerable<GameComponent> GetComponents() => _components.Values.SelectMany(x => x);
}