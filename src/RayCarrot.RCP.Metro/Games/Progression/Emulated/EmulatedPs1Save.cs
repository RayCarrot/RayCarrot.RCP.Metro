using BinarySerializer;
using BinarySerializer.PlayStation.PS1.MemoryCard;

namespace RayCarrot.RCP.Metro;

public class EmulatedPs1Save : EmulatedSave
{
    public EmulatedPs1Save(EmulatedSaveFile file, Context context, MemoryCard memoryCard, int block, string name) : base(file, context)
    {
        MemoryCard = memoryCard;
        Block = block;
        Name = name;
    }

    public MemoryCard MemoryCard { get; }
    public int Block { get; }
    public string Name { get; }

    public override async Task<T> ReadAsync<T>()
    {
        using (Context)
        {
            Pointer savePointer = MemoryCard.GetPointer(Block, 0);
            BinaryDeserializer s = Context.Deserializer;
            s.Goto(savePointer);
            return await Task.Run(() => s.SerializeObject<T>(default, name: "DataBlock"));
        }
    }

    public override Task WriteAsync<T>(T obj)
    {
        using (Context)
        {
            Pointer savePointer = MemoryCard.GetPointer(Block, 0);
            BinarySerializer.BinarySerializer s = Context.Serializer;
            s.Goto(savePointer);
            s.SerializeObject<T>(obj, name: "DataBlock");
        }

        return Task.CompletedTask;
    }
}