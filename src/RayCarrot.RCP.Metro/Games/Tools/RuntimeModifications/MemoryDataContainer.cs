namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class MemoryDataContainer
{
    #region Constructor

    public MemoryDataContainer(MemoryData memData)
    {
        _memData = memData;
    }

    #endregion

    #region Private Fields

    private readonly MemoryData _memData;

    #endregion

    #region Public Methods

    public T? AccessMemory<T>(Func<MemoryData, T> func)
    {
        lock (_memData)
            return func(_memData);
    }

    public void AccessMemory(Action<MemoryData> action)
    {
        lock (_memData)
            action(_memData);
    }

    public T? AccessMemory<TMemObj, T>(Func<TMemObj, T> func)
        where TMemObj : MemoryData
    {
        lock (_memData)
            return func((TMemObj)_memData);
    }

    public void AccessMemory<TMemObj>(Action<TMemObj> action)
        where TMemObj : MemoryData
    {
        lock (_memData)
            action((TMemObj)_memData);
    }

    public bool Validate()
    {
        lock (_memData)
            return _memData.Validate();
    }

    public void Update()
    {
        lock (_memData)
            _memData.Serialize();
    }

    #endregion
}