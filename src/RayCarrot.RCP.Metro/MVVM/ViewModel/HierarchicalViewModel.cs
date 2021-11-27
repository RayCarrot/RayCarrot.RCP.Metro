using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A hierarchical collection of view models identified by ID
/// </summary>
/// <typeparam name="T">The type of view models in the hierarchy</typeparam>
public class HierarchicalViewModel<T> : ObservableCollection<T>, IHierarchicalCollection
    where T : HierarchicalViewModel<T>
{
    #region Constructor

    /// <summary>
    /// Creates a new <see cref="HierarchicalViewModel{T}"/> from an item ID
    /// </summary>
    /// <param name="id">The ID of this item</param>
    /// <exception cref="ArgumentNullException"/>
    public HierarchicalViewModel(string id)
    {
        ID = id ?? throw new ArgumentNullException(nameof(id));
    }

    /// <summary>
    /// Creates a new <see cref="HierarchicalViewModel{T}"/> from a parent and item ID
    /// </summary>
    /// <param name="parent">The parent</param>
    /// <param name="id">The ID of this item</param>
    /// <exception cref="ArgumentNullException"/>
    public HierarchicalViewModel(T parent, string id)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        if (id == null)
            throw new ArgumentNullException(nameof(id));

        _fullID = parent.FullID.Append(id).ToArray();
        _parent = parent;
        ID = id;
    }

    #endregion

    #region Internal Fields

    internal string[]? _fullID;

    internal T? _parent;

    #endregion

    #region Public Properties

    /// <summary>
    /// True if there are sub items, false if not
    /// </summary>
    public virtual bool HasSubItems => Count > 0;

    /// <summary>
    /// The ID of this item
    /// </summary>
    public virtual string ID { get; }

    /// <summary>
    /// The ID including the ID's of the item's parents in order
    /// </summary>
    public virtual string[] FullID => _fullID ?? new string[] { ID };

    /// <summary>
    /// The parent view model
    /// </summary>
    public virtual T? Parent => _parent;

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a new view model to the collection
    /// </summary>
    /// <param name="item">The view model to add</param>
    public new virtual void Add(T item)
    {
        base.Add(item);

        item._fullID = FullID.Append(item.ID).ToArray();

        if (item.Parent == null)
            item._parent = this as T;
    }

    #endregion
}

/// <summary>
/// A hierarchical collection of view models identified by ID
/// </summary>
public class HierarchicalViewModel : HierarchicalViewModel<HierarchicalViewModel>
{
    #region Constructor

    /// <summary>
    /// Creates a new <see cref="HierarchicalViewModel"/> from an item ID
    /// </summary>
    /// <param name="id">The ID of this item</param>
    /// <exception cref="ArgumentNullException"/>
    public HierarchicalViewModel(string id) : base(id)
    {

    }

    /// <summary>
    /// Creates a new <see cref="HierarchicalViewModel"/> from a parent and item ID
    /// </summary>
    /// <param name="parent">The parent</param>
    /// <param name="id">The ID of this item</param>
    /// <exception cref="ArgumentNullException"/>
    public HierarchicalViewModel(HierarchicalViewModel parent, string id) : base(parent, id)
    {

    }

    #endregion
}