namespace RayCarrot.RCP.Metro.Games.Components;

public static class GameComponentExtensions
{
    public static IEnumerable<T> CreateObjects<T>(
        this IEnumerable<FactoryGameComponent<T>> factories) => factories.Select(x => x.CreateObject());

    public static IEnumerable<T> CreateManyObjects<T>(
        this IEnumerable<FactoryGameComponent<IEnumerable<T>>> factories) => factories.SelectMany(x => x.CreateObject());

    public static async Task InvokeAllAsync(
        this IEnumerable<AsyncActionGameComponent> actions)
    {
        foreach (AsyncActionGameComponent action in actions)
            await action.InvokeAsync();
    }

    public static async Task InvokeAllAsync<T>(
        this IEnumerable<AsyncActionGameComponent<T>> actions,
        T arg)
    {
        foreach (AsyncActionGameComponent<T> action in actions)
            await action.InvokeAsync(arg);
    }

    public static void InvokeAll(
        this IEnumerable<ActionGameComponent> actions)
    {
        foreach (ActionGameComponent action in actions)
            action.Invoke();
    }

    public static void InvokeAll<T>(
        this IEnumerable<ActionGameComponent<T>> actions,
        T arg)
    {
        foreach (ActionGameComponent<T> action in actions)
            action.Invoke(arg);
    }
}