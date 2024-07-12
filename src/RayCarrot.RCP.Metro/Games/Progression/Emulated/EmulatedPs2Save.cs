using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class EmulatedPs2Save : EmulatedSave
{
    protected EmulatedPs2Save(EmulatedSaveFile file, Context context) : base(file, context) { }

    public abstract Task<T> ReadAsync<T>(string fileName)
        where T : BinarySerializable, new();
}