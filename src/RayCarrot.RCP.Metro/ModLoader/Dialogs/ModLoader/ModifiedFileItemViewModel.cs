namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class ModifiedFileItemViewModel : BaseViewModel
{
    #region Constructor

    public ModifiedFileItemViewModel(string name) : this(name, null, ItemType.None, FileModification.None, false) { }

    public ModifiedFileItemViewModel(string name, string? mod, ItemType type, FileModification modification, bool hasInvalidLocation)
    {
        Name = name;
        Mod = mod;
        Type = type;
        Modification = modification;
        HasInvalidLocation = hasInvalidLocation;
    }

    #endregion

    #region Private Fields

    private readonly Dictionary<string, ModifiedFileItemViewModel> _itemsToAdd = new();

    #endregion

    #region Public Properties

    public ObservableCollectionEx<ModifiedFileItemViewModel> Children { get; } = new();
    public string Name { get; }
    public string? Mod { get; }
    public ObservableCollection<string?> OverridenMods { get; } = new();
    public ItemType Type { get; }
    public FileModification Modification { get; }
    public bool HasInvalidLocation { get; }

    public bool IsExpanded { get; set; }

    #endregion

    #region Public Methods

    public ModifiedFileItemViewModel? GetItem(string name)
    {
        return _itemsToAdd.TryGetValue(name.ToLowerInvariant());
    }

    public void AddItem(ModifiedFileItemViewModel item)
    {
        _itemsToAdd[item.Name.ToLowerInvariant()] = item;
    }

    public void ApplyItems()
    {
        Children.ModifyCollection(x =>
        {
            x.Clear();
            x.AddRange(_itemsToAdd.Values.OrderBy(item => item.Type).ThenBy(item => item.Name));
        });

        _itemsToAdd.Clear();

        foreach (ModifiedFileItemViewModel child in Children)
            child.ApplyItems();
    }

    #endregion

    #region Enums

    public enum ItemType
    {
        None,
        Folder,
        Archive,
        File,
    }

    public enum FileModification
    {
        None,
        Add,
        Remove,
    }

    #endregion
}