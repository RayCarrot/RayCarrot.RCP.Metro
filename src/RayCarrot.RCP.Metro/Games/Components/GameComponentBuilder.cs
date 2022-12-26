﻿using System.Reflection;

namespace RayCarrot.RCP.Metro.Games.Components;

// This class has several DEBUG conditions. These are mainly there to find easily
// preventable errors and don't need to run in RELEASE mode.

public class GameComponentBuilder
{
    private readonly List<Component> _components = new();

    private void Register(Component component)
    {
        GameComponentAttribute? attr = component.BaseType.GetCustomAttribute<GameComponentAttribute>();

#if DEBUG
        // Make sure the base type is correct
        if (attr is not { IsBase: true })
            throw new InvalidOperationException($"Can't register a component of type {component.BaseType} due to it not being a component base type");

        // Save the attribute for when we build
        component.Attribute = attr;
#endif

        // Check if it's a single-instance component
        if (attr.SingleInstance)
            _components.RemoveAll(x => x.BaseType == component.BaseType);

        _components.Add(component);
    }

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="component">The component to register</param>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public void Register<T>(T component, ComponentPriority priority = ComponentPriority.Normal) 
        where T : GameComponent
    {
        Register(new Component(typeof(T), typeof(T), component, priority));
    }

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public void Register<T>(ComponentPriority priority = ComponentPriority.Normal)
        where T : GameComponent, new()
    {
        Register(new Component(typeof(T), typeof(T), null, priority));
    }

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <typeparam name="U">The type of the component instance</typeparam>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public void Register<T, U>(ComponentPriority priority = ComponentPriority.Normal) 
        where T : GameComponent
        where U : T, new()
    {
        Register(new Component(typeof(T), typeof(U), null, priority));
    }

    /// <summary>
    /// Builds the components to a provider
    /// </summary>
    /// <returns></returns>
    public GameComponentProvider Build(GameInstallation gameInstallation)
    {
#if DEBUG
        // Verify all required components are there. This code is not
        // optimized at all as it will do multiple enumerations.
        foreach (Component component in _components)
        {
            if (component.Attribute?.RequiredComponents.Any() != true) 
                continue;
            
            foreach (Type type in component.Attribute.RequiredComponents)
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
#if DEBUG
        public GameComponentAttribute? Attribute { get; set; }
#endif
        public GameComponent GetInstance() => Instance ?? (GameComponent)Activator.CreateInstance(InstanceType);
    }
}