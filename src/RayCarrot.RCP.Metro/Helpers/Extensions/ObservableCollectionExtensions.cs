﻿using System.Windows.Data;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for an <see cref="ObservableCollection{T}"/>
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Adds a range of items to an <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection</typeparam>
    /// <param name="collection">The collection to add to</param>
    /// <param name="range">The range of items to add</param>
    /// <exception cref="ArgumentNullException"/>
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (range == null)
            throw new ArgumentNullException(nameof(range));

        foreach (T item in range)
            collection.Add(item);
    }

    public static void EnableCollectionSynchronization<T>(this ObservableCollection<T> collection)
    {
        // Ensure we're on the UI thread
        App.Current.Dispatcher.Invoke(() =>
        {
            // Enable synchronization with a new object to lock on
            BindingOperations.EnableCollectionSynchronization(collection, new object());
        });
    }
}