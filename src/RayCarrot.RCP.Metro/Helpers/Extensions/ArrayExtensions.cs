#nullable disable
using System;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for an array of items
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Append the given objects to the array
    /// </summary>
    /// <typeparam name="T">The type of array</typeparam>
    /// <param name="source">The original array of values</param>
    /// <param name="toAdd">The values to append to the source</param>
    /// <returns>The new array</returns>
    /// <exception cref="ArgumentNullException"/>
    public static T[] AppendToArray<T>(this T[] source, params T[] toAdd)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (toAdd == null)
            throw new ArgumentNullException(nameof(toAdd));

        return source.Concat(toAdd).ToArray();
    }

    /// <summary>
    /// Prepend the given objects to the array
    /// </summary>
    /// <typeparam name="T">The type of array</typeparam>
    /// <param name="source">The original array of values</param>
    /// <param name="toAdd">The values to prepend to the source</param>
    /// <returns>The new array</returns>
    /// <exception cref="ArgumentNullException"/>
    public static T[] PrependToArray<T>(this T[] source, params T[] toAdd)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (toAdd == null)
            throw new ArgumentNullException(nameof(toAdd));

        return toAdd.Concat(source).ToArray();
    }
}