using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public abstract class Game : BaseViewModel
{
    protected MemoryDataContainer? Container { get; private set; }

    public void AttachContainer(MemoryDataContainer container) => Container = container;
    public void DetachContainer() => Container = null;

    public abstract MemoryData CreateMemoryData(Context context);
    public abstract void InitializeContext(Context context);

    public abstract IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups();
    public abstract IEnumerable<DuoGridItemViewModel> CreateInfoItems();
    public abstract IEnumerable<ActionViewModel> CreateActions();
}

public abstract class Mod_Mem_Game<TMemObj> : Game
    where TMemObj : MemoryData, new()
{
    protected DuoGridItemViewModel DuoGridItem(LocalizedString header, Func<TMemObj, object?> textFunc) => 
        new(header, new GeneratedLocString(() => AccessMemory(m => $"{textFunc(m)}")));


    public T? AccessMemory<T>(Func<TMemObj, T> func) => Container != null ? Container.AccessMemory<TMemObj, T>(func) : default;
    public void AccessMemory(Action<TMemObj> action) => Container?.AccessMemory<TMemObj>(action);

    public override MemoryData CreateMemoryData(Context context) => new TMemObj();
}