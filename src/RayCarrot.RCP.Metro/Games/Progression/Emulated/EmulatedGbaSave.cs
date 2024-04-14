using BinarySerializer;
using BinarySerializer.Nintendo.GBA;

namespace RayCarrot.RCP.Metro;

public class EmulatedGbaSave : EmulatedSave
{
    public EmulatedGbaSave(EmulatedSaveFile file, Context context, string fileName) : base(file, context)
    {
        FileName = fileName;
    }

    public string FileName { get; }

    public override Task<T> ReadAsync<T>()
    {
        // For now we assume every save is a 512 byte EEPROM save. In the future we might want to make this optional.
        IStreamEncoder encoder = new EEPROMEncoder(0x200);

        using (Context)
            return Context.ReadRequiredFileDataAsync<T>(FileName, encoder, removeFileWhenComplete: false);
    }

    public override Task WriteAsync<T>(T obj)
    {
        using (Context)
            FileFactory.Write<T>(Context, FileName, obj);

        return Task.CompletedTask;
    }
}