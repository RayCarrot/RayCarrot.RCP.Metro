using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class EmulatedJaguarSave : EmulatedSave
{
    public EmulatedJaguarSave(EmulatedSaveFile file, Context context, string fileName) : base(file, context)
    {
        FileName = fileName;
    }

    public string FileName { get; }

    public override async Task<T> ReadAsync<T>()
    {
        using (Context)
            return await Context.ReadRequiredFileDataAsync<T>(FileName, endian: Endian.Big, removeFileWhenComplete: false);
    }

    public override Task WriteAsync<T>(T obj)
    {
        using (Context)
            FileFactory.Write<T>(Context, FileName, obj);

        return Task.CompletedTask;
    }
}