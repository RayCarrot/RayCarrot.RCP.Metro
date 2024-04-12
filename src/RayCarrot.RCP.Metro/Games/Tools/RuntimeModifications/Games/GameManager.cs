using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public abstract class GameManager
{
    protected GameManager(LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc)
    {
        DisplayName = displayName;
        _getOffsetsFunc = getOffsetsFunc;
    }

    private readonly Func<Dictionary<string, long>> _getOffsetsFunc;

    public LocalizedString DisplayName { get; }

    protected MemoryDataContainer? Container { get; private set; }

    public Dictionary<string, long> GetOffsets() => _getOffsetsFunc();

    public void AttachContainer(MemoryDataContainer container) => Container = container;
    public void DetachContainer() => Container = null;

    public abstract MemoryData CreateMemoryData(Context context);
    public virtual void InitializeContext(Context context) { }

    public abstract IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups(Context context);
    public abstract IEnumerable<DuoGridItemViewModel> CreateInfoItems(Context context);
    public abstract IEnumerable<ActionViewModel> CreateActions(Context context);
}

public abstract class GameManager<TMemObj> : GameManager
    where TMemObj : MemoryData, new()
{
    protected GameManager(LocalizedString displayName, Func<Dictionary<string, long>> getOffsetsFunc) 
        : base(displayName, getOffsetsFunc) { }

    protected DuoGridItemViewModel DuoGridItem(LocalizedString header, Func<TMemObj, object?> textFunc) => 
        new(header, new GeneratedLocString(() => AccessMemory(m => $"{textFunc(m)}")));

    public T? AccessMemory<T>(Func<TMemObj, T> func) => Container != null ? Container.AccessMemory<TMemObj, T>(func) : default;
    public void AccessMemory(Action<TMemObj> action) => Container?.AccessMemory<TMemObj>(action);

    public override MemoryData CreateMemoryData(Context context) => new TMemObj();
}