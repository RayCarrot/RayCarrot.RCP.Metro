namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="ICollection{T}"/>
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Removes all items in the collection which matches the predicate
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <param name="collection">The collection to modify</param>
    /// <param name="predicate">The predicate used to determine if an item should be removed</param>
    public static void RemoveWhere<T>(this ICollection<T> collection, Predicate<T> predicate)
    {
        foreach (T item in collection.Where(x => predicate(x)).ToArray())
            collection.Remove(item);
    }

    // TODO: Remove this when we migrate to dotnet8. It's only faster on older frameworks. See benchmark:
    // | Method    | Job                  | Runtime              | Mean      | Error     | StdDev    | Median    | Gen0    | Gen1   | Allocated |
    // |---------- |--------------------- |--------------------- |----------:|----------:|----------:|----------:|--------:|-------:|----------:|
    // | Linq      | .NET 8.0             | .NET 8.0             |  9.449 us | 0.3118 us | 0.9193 us |  8.920 us |  9.5215 | 1.3580 |  78.22 KB |
    // | Extension | .NET 8.0             | .NET 8.0             | 38.283 us | 0.3404 us | 0.2843 us | 38.290 us |  9.5215 |      - |  78.17 KB |
    // | Linq      | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 67.419 us | 1.0714 us | 0.9498 us | 67.052 us | 33.5693 | 4.1504 | 206.84 KB |
    // | Extension | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 49.551 us | 0.9302 us | 0.9136 us | 49.194 us | 12.6343 |      - |  78.22 KB |
    public static TResult[] ToArray<TSource, TResult>(this IList<TSource> collection, Func<TSource, TResult> selector)
    {
        TResult[] resultArray = new TResult[collection.Count];

        for (int i = 0; i < collection.Count; i++)
        {
            resultArray[i] = selector(collection[i]);
        }

        return resultArray;
    }

    public static TResult[] ToArray<TSource, TResult>(this IList<TSource> collection, Func<TSource, int, TResult> selector)
    {
        TResult[] resultArray = new TResult[collection.Count];

        for (int i = 0; i < collection.Count; i++)
        {
            resultArray[i] = selector(collection[i], i);
        }

        return resultArray;
    }
}