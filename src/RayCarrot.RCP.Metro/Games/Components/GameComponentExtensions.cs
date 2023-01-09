using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Components;

public static class GameComponentExtensions
{
    public static IEnumerable<T> CreateObjects<T>(
        this IEnumerable<FactoryGameComponent<T>> factories) => factories.Select(x => x.CreateObject());

    public static IEnumerable<T> CreateManyObjects<T>(
        this IEnumerable<FactoryGameComponent<IEnumerable<T>>> factories) => factories.SelectMany(x => x.CreateObject());

    public static async Task InvokeAllAsync(
        this IEnumerable<ActionGameComponent> actions)
    {
        foreach (ActionGameComponent action in actions)
            await action.InvokeAsync();
    }

    public static void InitContext(this IEnumerable<InitializeContextComponent> settings, Context context)
    {
        foreach (InitializeContextComponent contextComponent in settings)
            contextComponent.InitContext(context);
    }
}