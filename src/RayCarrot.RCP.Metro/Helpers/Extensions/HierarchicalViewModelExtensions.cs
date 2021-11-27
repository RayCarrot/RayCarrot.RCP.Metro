using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="HierarchicalViewModel{T}"/>
/// </summary>
public static class HierarchicalViewModelExtensions
{
    /// <summary>
    /// Gets a list of hierarchical view model from a <see cref="HierarchicalViewModel{T}"/> recursively
    /// </summary>
    /// <param name="parent">The parent view model to get the child view models from</param>
    /// <param name="includeSelf">True if the parent should be included, false if not</param>
    /// <returns>The list of nodes</returns>
    public static IEnumerable<T> GetAllChildren<T>(this T parent, bool includeSelf = false)
        where T : HierarchicalViewModel<T>
    {
        if (includeSelf)
            yield return parent;

        foreach (T child in parent)
        {
            if (child == null)
                continue;

            foreach (T subChild in child.GetAllChildren(true))
                yield return subChild;
        }
    }
}