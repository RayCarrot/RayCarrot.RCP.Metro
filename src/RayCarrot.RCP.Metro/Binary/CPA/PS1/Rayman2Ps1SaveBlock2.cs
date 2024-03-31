#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Rayman2Ps1SaveBlock2 : Rayman2Ps1SaveBlock
{
    public override int AllocatedSize => 0x10;
    public override int ExpectedSize => 0x1;

    // Bit 1: Vibration enabled
    public byte Flags { get; set; }

    protected override void SerializeData(SerializerObject s)
    {
        Flags = s.Serialize<byte>(Flags, name: nameof(Flags));
    }
}