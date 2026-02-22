#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Save data for Rayman 2 (GBC)
/// </summary>
public class R2GBC_SaveData : BinarySerializable
{
    public bool[] ValidSlots { get; set; }
    public R2GBC_SaveSlot[] Slots { get; set; }
    public byte SlotIndex { get; set; } // Not really sure how this is used

    public override void SerializeImpl(SerializerObject s)
    {
        s.SerializeMagicString("RAYMAN2", 7);
        ValidSlots = s.SerializeArray<bool>(ValidSlots, 4, name: nameof(ValidSlots));
        Slots = s.SerializeObjectArray<R2GBC_SaveSlot>(Slots, 4, name: nameof(Slots));
        SlotIndex = s.Serialize<byte>(SlotIndex, name: nameof(SlotIndex));
    }
}