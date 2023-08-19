using System.Windows;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="DependencyObject"/>
/// </summary>
public static class DependencyObjectExtensions
{
    /// <summary>
    /// Finds the first parent of specified type
    /// </summary>
    /// <typeparam name="T">The type of parent control to find</typeparam>
    /// <param name="child">The object to search from</param>
    /// <returns>The parent of the specified type, or null if none was found</returns>
    public static T? FindParent<T>(this DependencyObject child)
        where T : DependencyObject
    {
        DependencyObject? item = child.GetParentObject();

        while (item != null)
        {
            if (item is T itemAsT)
                return itemAsT;

            item = item.GetParentObject();
        }

        return null;
    }
}