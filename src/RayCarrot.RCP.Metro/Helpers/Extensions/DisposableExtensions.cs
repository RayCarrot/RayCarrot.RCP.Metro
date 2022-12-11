#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="IDisposable"/>
/// </summary>
public static class DisposableExtensions
{
    /// <summary>
    /// Runs the action and disposes the item once complete or if an exception is thrown
    /// </summary>
    /// <typeparam name="T">The type of disposable item</typeparam>
    /// <param name="item">The disposable item</param>
    /// <param name="action">The action to run</param>
    /// <exception cref="ArgumentNullException"/>
    public static void RunAndDispose<T>(this T item, Action<T> action)
        where T : IDisposable
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        using (item)
            action.Invoke(item);
    }

    /// <summary>
    /// Runs the action and disposes the item once complete or if an exception is thrown
    /// </summary>
    /// <typeparam name="T">The type of disposable item</typeparam>
    /// <typeparam name="R">The return value</typeparam>
    /// <param name="item">The disposable item</param>
    /// <param name="action">The action to run</param>
    /// <exception cref="ArgumentNullException"/>
    public static R RunAndDispose<T, R>(this T item, Func<T, R> action)
        where T : IDisposable
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        using (item)
            return action.Invoke(item);
    }
}