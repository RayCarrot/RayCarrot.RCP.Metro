#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRRGBA_SaveSlot : BinarySerializable
{
    public string Name { get; set; }
    public byte[] Levels { get; set; }
    public byte[] Cages { get; set; }
    public byte[] Lums { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        s.SerializePadding(1, logIfNotNull: true);
        Name = s.SerializeString(Name, length: 3, name: nameof(Name));
        Levels = s.SerializeArray<byte>(Levels, 4, name: nameof(Levels));
        Cages = s.SerializeArray<byte>(Cages, 12, name: nameof(Cages));
        Lums = s.SerializeArray<byte>(Lums, 196, name: nameof(Lums));
    }
}