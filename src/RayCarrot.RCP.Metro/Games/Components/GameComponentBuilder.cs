using System.Reflection;

namespace RayCarrot.RCP.Metro.Games.Components;

// This class has several DEBUG conditions. These are mainly there to find easily
// preventable errors and aren't run in RELEASE mode for performance reasons.

public class GameComponentBuilder : IGameComponentBuilder
{
    private bool _hasBuilt;
    private readonly List<Component> _components = new();

    public void Register(Type baseType, Type instanceType, GameComponent? instance, ComponentPriority priority)
    {
#if DEBUG
        // Make sure the base type has the attribute
        if (baseType.GetCustomAttribute<BaseGameComponentAttribute>() == null)
            throw new InvalidOperationException($"Can't register a component of type {baseType} due to it not being a component base type");
#endif

        // Check if it's a single-instance component
        if (instanceType.GetCustomAttribute<SingleInstanceGameComponentAttribute>() != null)
            _components.RemoveAll(x => x.BaseType == baseType);

        // Handle default priority
        if (priority == ComponentPriority.Default)
            priority = instanceType.GetCustomAttribute<DefaultGameComponentPriorityAttribute>()?.DefaultPriority ??
                       ComponentPriority.Normal;

        _components.Add(new Component(baseType, instanceType, instance, priority));
    }

    /// <summary>
    /// Builds the components and returns them
    /// </summary>
    /// <returns>The built components</returns>
    public IEnumerable<Component> Build()
    {
        if (_hasBuilt)
            throw new InvalidOperationException("The components can only be built once");

        _hasBuilt = true;

        // Register components from components
        do
        {
            foreach (Component component in _components.ToArray())
            {
                if (!component.HasRegisteredComponents)
                {
                    component.GetInstance().RegisterComponents(this);
                    component.HasRegisteredComponents = true;
                }
            }
        } while (_components.Any(x => !x.HasRegisteredComponents));

#if DEBUG
        // Verify all required components are there. This code is not
        // optimized at all as it will do multiple enumerations.
        foreach (Component component in _components)
        {
            foreach (Type type in component.InstanceType.GetCustomAttributes<RequiredGameComponentsAttribute>().
                         SelectMany(x => x.RequiredComponents))
            {
                if (_components.All(x => x.BaseType != type))
                    throw new Exception($"The required component type {type.FullName} has not been registered");
            }
        }
#endif

        return _components.OrderByDescending(x => x.Priority);
    }

    public record Component(Type BaseType, Type InstanceType, GameComponent? Instance, ComponentPriority Priority)
    {
        private GameComponent? Instance { get; set; } = Instance;
        public bool HasRegisteredComponents { get; set; }
        public GameComponent GetInstance() => Instance ??= (GameComponent)Activator.CreateInstance(InstanceType);
    }
}