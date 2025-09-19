using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR_SaveSlot : BinarySerializable
{
    public RRR_SavSlotDesc SlotDesc { get; set; }
    public RRR_UniversSave Univers { get; set; } // Stored as a save buffer in memory with a size of 0x11440

    public override void SerializeImpl(SerializerObject s)
    {
        SlotDesc = s.SerializeObject<RRR_SavSlotDesc>(SlotDesc, name: nameof(SlotDesc));
        long pos = s.CurrentFileOffset;
        Univers = s.SerializeObject<RRR_UniversSave>(Univers, name: nameof(Univers));
        s.Goto(new Pointer(pos + 0x11440, Offset.File));
    }
}