using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="List{T}"/>
/// </summary>
public static class ListExtensions
{
    public static void AddSorted<T>(this List<T> list, T item)
        where T : IComparable<T>
    {
        if (list.Count == 0)
        {
            list.Add(item);
            return;
        }

        if (list[list.Count - 1].CompareTo(item) <= 0)
        {
            list.Add(item);
            return;
        }

        if (list[0].CompareTo(item) >= 0)
        {
            list.Insert(0, item);
            return;
        }

        int index = list.BinarySearch(item);
        
        if (index < 0)
            index = ~index;
        
        list.Insert(index, item);
    }
}