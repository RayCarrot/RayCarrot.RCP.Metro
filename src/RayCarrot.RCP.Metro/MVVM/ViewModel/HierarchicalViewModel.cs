using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

// TODO-UPDATE: Remove

/// <summary>
/// A hierarchical collection of view models identified by ID
/// </summary>
/// <typeparam name="T">The type of view models in the hierarchy</typeparam>
public class HierarchicalViewModel<T> : ObservableCollection<T>, IHierarchicalCollection
    where T : HierarchicalViewModel<T>
{
    #region Constructor

    /// <summary>
    /// Creates a new <see cref="HierarchicalViewModel{T}"/> from a parent and item ID
    /// </summary>
    /// <param name="parent">The parent</param>
    /// <param name="id">The ID of this item</param>
    /// <exception cref="ArgumentNullException"/>
    public HierarchicalViewModel(T? parent, string id)
    {
        ID = id ?? throw new ArgumentNullException(nameof(id));
        FullID = parent?.FullID.Append(id).ToArray() ?? ID.YieldToArray();
        Parent = parent;
    }

    #endregion

    #region Private Fields

    #endregion

    #region Public Properties

    /// <summary>
    /// True if there are sub items, false if not
    /// </summary>
    public bool HasSubItems => Count > 0;

    /// <summary>
    /// The ID of this item
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// The ID including the ID's of the item's parents in order
    /// </summary>
    public string[] FullID { get; }

    /// <summary>
    /// The parent view model
    /// </summary>
    public T? Parent { get; }

    #endregion
}