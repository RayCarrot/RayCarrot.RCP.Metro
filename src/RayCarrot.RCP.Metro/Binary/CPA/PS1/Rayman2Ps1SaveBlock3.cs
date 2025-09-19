using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Rayman2Ps1SaveBlock3 : CPAPs1SaveBlock
{
    public override int AllocatedSize => 0x40;
    public override int ExpectedSize => 4;

    public int Language { get; set; }

    protected override void SerializeData(SerializerObject s)
    {
        Language = s.Serialize<int>(Language, name: nameof(Language));
    }
}