#nullable disable
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class RRR_SaveSlot : IBinarySerializable
{
    public RRR_SavSlotDesc SlotDesc { get; set; }
    public RRR_UniversSave Univers { get; set; } // Stored as a save buffer in memory with a size of 0x11440

    public void Serialize(IBinarySerializer s)
    {
        SlotDesc = s.SerializeObject<RRR_SavSlotDesc>(SlotDesc, name: nameof(SlotDesc));
        long pos = s.Stream.Position;
        Univers = s.SerializeObject<RRR_UniversSave>(Univers, name: nameof(Univers));
        s.Stream.Position += 0x11440 - (s.Stream.Position - pos);
    }
}