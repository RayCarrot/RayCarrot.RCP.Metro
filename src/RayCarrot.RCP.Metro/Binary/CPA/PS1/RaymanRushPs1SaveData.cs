using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RaymanRushPs1SaveData : BinarySerializable
{
    public RaymanRushPs1SaveBlock1 SaveBlock1 { get; set; }
    public RaymanRushPs1SaveBlock2 SaveBlock2 { get; set; }
    public RaymanRushPs1SaveBlock3 SaveBlock3 { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        SaveBlock1 = s.SerializeObject<RaymanRushPs1SaveBlock1>(SaveBlock1, name: nameof(SaveBlock1));
        SaveBlock2 = s.SerializeObject<RaymanRushPs1SaveBlock2>(SaveBlock2, name: nameof(SaveBlock2));
        SaveBlock3 = s.SerializeObject<RaymanRushPs1SaveBlock3>(SaveBlock3, name: nameof(SaveBlock3));
        
        s.Goto(Offset + 0x400 - 4);
        int checksum = s.Serialize<int>(SaveBlock1.Checksum, name: "SaveBlock1Checksum");
        if (checksum != SaveBlock1.Checksum)
            s.SystemLogger?.LogWarning("Checksum fields don't match");
    }
}