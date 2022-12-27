using System.Reflection;

namespace RayCarrot.RCP.Metro.Games.Components;

// This class has several DEBUG conditions. These are mainly there to find easily
// preventable errors and aren't run in RELEASE mode for performance reasons.

public class GameComponentBuilder : IGameComponentBuilder
{
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

        _components.Add(new Component(baseType, instanceType, instance, priority));
    }

    /// <summary>
    /// Builds the components to a provider
    /// </summary>
    /// <returns>The provider</returns>
    public GameComponentProvider Build(GameInstallation gameInstallation)
    {
        // TODO-14: This should probably be recursive - but how?
        // Register components from components
        foreach (Component component in _components.ToArray())
            component.GetInstance().RegisterComponents(this);

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

        return new GameComponentProvider(_components.
            OrderByDescending(x => x.Priority).
            Select(x => (x.BaseType, x.GetInstance())), gameInstallation);
    }

    private record Component(Type BaseType, Type InstanceType, GameComponent? Instance, ComponentPriority Priority)
    {
        private GameComponent? Instance { get; set; } = Instance;
        public GameComponent GetInstance() => Instance ??= (GameComponent)Activator.CreateInstance(InstanceType);
    }
}