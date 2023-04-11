#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Save data for Rayman 3 (GBA)
/// </summary>
public class R3GBA_SaveData : BinarySerializable
{
    public R3GBA_SaveSlot[] Slots { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Slots = s.SerializeObjectArray<R3GBA_SaveSlot>(Slots, 3, name: nameof(Slots));
    }
}