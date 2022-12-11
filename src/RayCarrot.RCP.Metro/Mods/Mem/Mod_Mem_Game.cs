using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Mod_Mem_Game : BaseViewModel
{
    protected Mod_Mem_MemoryDataContainer? Container { get; private set; }

    public void AttachContainer(Mod_Mem_MemoryDataContainer container) => Container = container;
    public void DetachContainer() => Container = null;

    public abstract Mod_Mem_MemoryData CreateMemoryData(Context context);
    public abstract void InitializeContext(Context context);

    public abstract IEnumerable<EditorFieldGroupViewModel> CreateEditorFieldGroups();
    public abstract IEnumerable<DuoGridItemViewModel> CreateInfoItems();
    public abstract IEnumerable<Mod_Mem_ActionViewModel> CreateActions();
}

public abstract class Mod_Mem_Game<TMemObj> : Mod_Mem_Game
    where TMemObj : Mod_Mem_MemoryData, new()
{
    protected DuoGridItemViewModel DuoGridItem(LocalizedString header, Func<TMemObj, object?> textFunc) => 
        new(header, new GeneratedLocString(() => AccessMemory(m => $"{textFunc(m)}")));


    public T? AccessMemory<T>(Func<TMemObj, T> func) => Container != null ? Container.AccessMemory<TMemObj, T>(func) : default;
    public void AccessMemory(Action<TMemObj> action) => Container?.AccessMemory<TMemObj>(action);

    public override Mod_Mem_MemoryData CreateMemoryData(Context context) => new TMemObj();
}