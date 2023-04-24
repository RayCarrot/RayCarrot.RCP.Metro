using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class EmulatedSave
{
    protected EmulatedSave(EmulatedSaveFile file, Context context)
    {
        File = file;
        Context = context;
    }

    public EmulatedSaveFile File { get; }
    public Context Context { get; }

    public abstract Task<T> ReadAsync<T>()
        where T : BinarySerializable, new();
    public abstract Task WriteAsync<T>(T obj)
        where T : BinarySerializable, new();
}