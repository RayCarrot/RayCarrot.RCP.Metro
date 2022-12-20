namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="List{T}"/>
/// </summary>
public static class ListExtensions
{
    public static void AddSorted<T>(this List<T> list, T item)
        where T : IComparable<T>
    {
        AddSorted(list, item, Comparer<T>.Default);
    }

    public static void AddSorted<T>(this List<T> list, T item, IComparer<T> comparer)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        if (list.Count == 0)
        {
            list.Add(item);
            return;
        }

        if (comparer.Compare(list[list.Count - 1], item) <= 0)
        {
            list.Add(item);
            return;
        }

        if (comparer.Compare(list[0], item) >= 0)
        {
            list.Insert(0, item);
            return;
        }

        int index = list.BinarySearch(item, comparer);

        if (index < 0)
            index = ~index;

        list.Insert(index, item);
    }

    /// <summary>
    /// Searches within the sorted <see cref="IList{T}"/> for the
    /// specified item and returns the zero-based index of the item if found;
    /// otherwise, a negative number that is the bitwise complement of the index of
    /// the next item that is larger than item or, if there is no larger item,
    /// the bitwise complement of <see cref="IList{T}.Count"/>.
    /// </summary>
    /// <param name="list">The list to search</param>
    /// <param name="item">The item to find the index for</param>
    public static int SortedBinarySearch<T>(this IList<T> list, T item)
        where T : IComparable<T>
    {
        return SortedBinarySearch<T>(list, item, Comparer<T>.Default);
    }

    /// <summary>
    /// Searches within the sorted <see cref="IList{T}"/> for the
    /// specified item and returns the zero-based index of the item if found;
    /// otherwise, a negative number that is the bitwise complement of the index of
    /// the next item that is larger than item or, if there is no larger item,
    /// the bitwise complement of <see cref="IList{T}.Count"/>.
    /// </summary>
    /// <param name="list">The list to search</param>
    /// <param name="item">The item to find the index for</param>
    /// <param name="comparer">The comparer to use when searching the list</param>
    public static int SortedBinarySearch<T>(this IList<T> list, T item, IComparer<T> comparer)
    {
        if (list == null) 
            throw new ArgumentNullException(nameof(list));

        int lo = 0;
        int hi = list.Count - 1;
        while (lo <= hi)
        {
            int i = lo + ((hi - lo) >> 1);
            int order = comparer.Compare(list[i], item);

            if (order == 0) 
                return i;
            
            if (order < 0)
                lo = i + 1;
            else
                hi = i - 1;
        }
        return ~lo;
    }
}