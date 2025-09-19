#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RHR_SaveSlot : BinarySerializable
{
    public RHR_SaveSlotFlags Flags { get; set; }
    public int Score { get; set; }
    public short Lums { get; set; }
    public byte Teensies { get; set; }
    public string Name { get; set; }
    public RHR_LevelSave[] Levels { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoProcessed(new Checksum16Processor(), p =>
        {
            p?.Serialize<ushort>(s, name: "SaveSlotChecksum");
            s.SerializeMagic<byte>(0x47);

            Flags = s.Serialize<RHR_SaveSlotFlags>(Flags, name: nameof(Flags));
            Score = s.Serialize<int>(Score, name: nameof(Score));
            Lums = s.Serialize<short>(Lums, name: nameof(Lums));
            Teensies = s.Serialize<byte>(Teensies, name: nameof(Teensies));
            s.SerializePadding(1, logIfNotNull: true);
            Name = s.SerializeString(Name, length: 8, name: nameof(Name));
            Levels = s.SerializeObjectArray<RHR_LevelSave>(Levels, 20, name: nameof(Levels));
        });
    }
}