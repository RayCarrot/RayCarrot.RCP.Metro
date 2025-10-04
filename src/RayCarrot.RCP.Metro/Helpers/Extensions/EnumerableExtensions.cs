namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for an <see cref="IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Runs an action on each element of the enumerable object
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable object</typeparam>
    /// <param name="enumerable">The enumerable object to enumerate through</param>
    /// <param name="action">The action to run on each item</param>
    /// <exception cref="ArgumentNullException"/>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));

        if (action == null)
            throw new ArgumentNullException(nameof(action));
            
        foreach (T item in enumerable)
            action(item);
    }

    /// <summary>
    /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of 
    /// type <see cref="String"/>, using the specified separator between each member
    /// </summary>
    /// <typeparam name="T">The type of objects in the enumerable</typeparam>
    /// <param name="enumerable">The enumerable</param>
    /// <param name="seperator">The separator to use between each object</param>
    /// <returns>A string that consists of the members of values delimited by the separator string.
    /// If values has no members, the method returns <see cref="String.Empty"/>.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static string JoinItems<T>(this IEnumerable<T> enumerable, string seperator)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));

        if (seperator == null)
            throw new ArgumentNullException(nameof(seperator));

        return String.Join(seperator, enumerable);
    }

    /// <summary>
    /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of 
    /// type <see cref="String"/>, using the specified separator between each member
    /// </summary>
    /// <typeparam name="T">The type of objects in the enumerable</typeparam>
    /// <param name="enumerable">The enumerable</param>
    /// <param name="seperator">The separator to use between each object</param>
    /// <param name="formatter">The formatter to use for converting each object to a string</param>
    /// <returns>A string that consists of the members of values delimited by the separator string.
    /// If values has no members, the method returns <see cref="String.Empty"/>.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static string JoinItems<T>(this IEnumerable<T> enumerable, string seperator, Func<T, string> formatter)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));

        if (seperator == null)
            throw new ArgumentNullException(nameof(seperator));

        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        return String.Join(seperator, enumerable.Select(formatter));
    }

    /// <summary>
    /// Converts to an observable collection
    /// </summary>
    /// <typeparam name="T">The type of objects in the enumerable</typeparam>
    /// <param name="enumerable">The enumerable</param>
    /// <returns>The observable collection</returns>
    /// <exception cref="ArgumentNullException"/>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));

        if (enumerable is ObservableCollection<T> oc)
            return oc;

        return new ObservableCollection<T>(enumerable);
    }

    /// <summary>
    /// Returns the index of an item matching the predicate in a list
    /// </summary>
    /// <typeparam name="T">The type of objects in the list</typeparam>
    /// <param name="list">The list</param>
    /// <param name="match">The predicate used to find the matching item index</param>
    /// <returns>The item index matching the predicate, or -1 if none was found</returns>
    /// <exception cref="ArgumentNullException"/>
    public static int FindItemIndex<T>(this IList<T> list, Predicate<T> match)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        if (match == null)
            throw new ArgumentNullException(nameof(match));

        for (int i = 0; i < list.Count; i++)
            if (match(list[i]))
                return i;

        return -1;
    }

    /// <summary>
    /// Disposes all items in the collection
    /// </summary>
    /// <param name="disposables">The collection of disposable items</param>
    public static void DisposeAll(this IEnumerable<IDisposable>? disposables)
    {
        disposables?.ForEach(x => x?.Dispose());
    }
}