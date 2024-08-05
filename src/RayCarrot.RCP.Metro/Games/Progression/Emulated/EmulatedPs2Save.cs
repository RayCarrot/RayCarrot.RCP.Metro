using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class EmulatedPs2Save : EmulatedSave
{
    protected EmulatedPs2Save(EmulatedSaveFile file, Context context, string primaryFileName) : base(file, context)
    {
        PrimaryFileName = primaryFileName;
    }

    public string PrimaryFileName { get; }

    public abstract Task<T> ReadAsync<T>(string fileName)
        where T : BinarySerializable, new();
}