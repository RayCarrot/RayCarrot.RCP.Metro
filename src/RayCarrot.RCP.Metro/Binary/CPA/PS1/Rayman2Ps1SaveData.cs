#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Rayman2Ps1SaveData : BinarySerializable
{
    public Rayman2Ps1SaveBlock1 SaveBlock1 { get; set; }
    public Rayman2Ps1SaveBlock2 SaveBlock2 { get; set; }
    public Rayman2Ps1SaveBlock3 SaveBlock3 { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        SaveBlock1 = s.SerializeObject<Rayman2Ps1SaveBlock1>(SaveBlock1, name: nameof(SaveBlock1));
        SaveBlock2 = s.SerializeObject<Rayman2Ps1SaveBlock2>(SaveBlock2, name: nameof(SaveBlock2));
        SaveBlock3 = s.SerializeObject<Rayman2Ps1SaveBlock3>(SaveBlock3, name: nameof(SaveBlock3));
        
        s.Goto(Offset + 0x400);
    }
}