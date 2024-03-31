#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Rayman2Ps1SaveBlock : BinarySerializable
{
    public abstract int AllocatedSize { get; }
    public abstract int ExpectedSize { get; }
    public int Size { get; set; }

    protected abstract void SerializeData(SerializerObject s);

    public override void SerializeImpl(SerializerObject s)
    {
        Size = s.Serialize<int>(Size, name: nameof(Size));
        SerializeData(s);
        s.Goto(Offset + 4 + AllocatedSize);
    }
}