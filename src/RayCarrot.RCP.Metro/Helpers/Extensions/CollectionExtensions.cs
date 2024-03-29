﻿#nullable disable
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

    public static TResult[] ToArray<TSource, TResult>(this IList<TSource> collection, Func<TSource, TResult> selector)
    {
        TResult[] resultArray = new TResult[collection.Count];

        for (int i = 0; i < collection.Count; i++)
        {
            resultArray[i] = selector(collection[i]);
        }

        return resultArray;
    }
}