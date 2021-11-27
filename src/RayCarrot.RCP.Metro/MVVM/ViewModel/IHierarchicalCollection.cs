using System.Collections;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Represents a hierarchical collection where each element is identified
/// </summary>
public interface IHierarchicalCollection : IEnumerable, ICollection, IList
{
    /// <summary>
    /// True if there are sub items, false if not
    /// </summary>
    bool HasSubItems { get; }

    /// <summary>
    /// The ID of this item
    /// </summary>
    string ID { get; }

    /// <summary>
    /// The ID including the ID's of the item's parents in order
    /// </summary>
    string[] FullID { get; }
}