namespace RayCarrot.RCP.Metro.Games.Components;

public static class GameComponentBuilderExtensions
{
    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="builder">The component builder to register to</param>
    /// <param name="component">The component to register</param>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public static void Register<T>(this IGameComponentBuilder builder, T component, ComponentPriority priority = ComponentPriority.Normal)
        where T : GameComponent
    {
        builder.Register(typeof(T), typeof(T), component, priority);
    }

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <param name="builder">The component builder to register to</param>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public static void Register<T>(this IGameComponentBuilder builder, ComponentPriority priority = ComponentPriority.Normal)
        where T : GameComponent, new()
    {
        builder.Register(typeof(T), typeof(T), null, priority);
    }

    /// <summary>
    /// Registers a new component
    /// </summary>
    /// <typeparam name="T">The type to register the component as</typeparam>
    /// <typeparam name="U">The type of the component instance</typeparam>
    /// <param name="builder">The component builder to register to</param>
    /// <param name="priority">The component priority, to be used when there are several registered for the same type</param>
    public static void Register<T, U>(this IGameComponentBuilder builder, ComponentPriority priority = ComponentPriority.Normal)
        where T : GameComponent
        where U : T, new()
    {
        builder.Register(typeof(T), typeof(U), null, priority);
    }
}