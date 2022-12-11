namespace RayCarrot.RCP.Metro;

public class Mod_Mem_MemoryDataContainer
{
    #region Constructor

    public Mod_Mem_MemoryDataContainer(Mod_Mem_MemoryData memData)
    {
        _memData = memData;
    }

    #endregion

    #region Private Fields

    private readonly Mod_Mem_MemoryData _memData;

    #endregion

    #region Public Methods

    public T? AccessMemory<T>(Func<Mod_Mem_MemoryData, T> func)
    {
        lock (_memData)
            return func(_memData);
    }

    public void AccessMemory(Action<Mod_Mem_MemoryData> action)
    {
        lock (_memData)
            action(_memData);
    }

    public T? AccessMemory<TMemObj, T>(Func<TMemObj, T> func)
        where TMemObj : Mod_Mem_MemoryData
    {
        lock (_memData)
            return func((TMemObj)_memData);
    }

    public void AccessMemory<TMemObj>(Action<TMemObj> action)
        where TMemObj : Mod_Mem_MemoryData
    {
        lock (_memData)
            action((TMemObj)_memData);
    }

    public void Update()
    {
        lock (_memData)
            _memData.Serialize();
    }

    #endregion
}