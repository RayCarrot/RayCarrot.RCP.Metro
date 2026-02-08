using BinarySerializer;
using BinarySerializer.Nintendo.GCN;

namespace RayCarrot.RCP.Metro;

public class EmulatedGameCubeSave : EmulatedSave
{
    public EmulatedGameCubeSave(EmulatedSaveFile file, Context context, MemoryCardFileHeader memoryCardFileHeader, string fileName) : base(file, context)
    {
        MemoryCardFileHeader = memoryCardFileHeader;
        FileName = fileName;
    }

    public MemoryCardFileHeader MemoryCardFileHeader { get; }
    public string FileName { get; }

    public override async Task<T> ReadAsync<T>()
    {
        using (Context)
        {
            Pointer fileDataPointer = MemoryCardFileHeader.Offset + MemoryCardFileHeader.SerializedSize;
            BinaryDeserializer s = Context.Deserializer;
            s.Goto(fileDataPointer);
            return await Task.Run(() => s.SerializeObject<T>(default, name: "FileData"));
        }
    }

    public override Task WriteAsync<T>(T obj)
    {
        using (Context)
        {
            Pointer fileDataPointer = MemoryCardFileHeader.Offset + MemoryCardFileHeader.SerializedSize;
            BinarySerializer.BinarySerializer s = Context.Serializer;
            s.Goto(fileDataPointer);
            s.SerializeObject<T>(obj, name: "FileData");
        }

        return Task.CompletedTask;
    }
}