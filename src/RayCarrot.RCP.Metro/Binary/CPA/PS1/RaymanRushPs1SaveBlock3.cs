#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RaymanRushPs1SaveBlock3 : CPAPs1SaveBlock
{
    public override int AllocatedSize => 0x40;
    public override int ExpectedSize => -1; // Unknown size

    public byte[] Data { get; set; }

    protected override void SerializeData(SerializerObject s)
    {
        Data = s.SerializeArray<byte>(Data, Size, name: nameof(Data));
    }
}