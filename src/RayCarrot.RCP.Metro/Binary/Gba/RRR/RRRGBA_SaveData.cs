using BinarySerializer;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Save data for Rayman Raving Rabbids (GBA)
/// </summary>
public class RRRGBA_SaveData : BinarySerializable
{
    public RRRGBA_SaveSlot[] Slots { get; set; }
    public uint Flags { get; set; }
    public byte[] Bytes_1B4 { get; set; } // Appears unused?
    public ushort[] LevelTimes { get; set; } // In seconds
    public byte[] Bytes_1F2 { get; set; } // Appears unused?

    public byte SelectedSlot { get; set; } // Why is this in the save?

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoProcessed(new Checksum32Processor(valueSize: 4), p =>
        {
            Slots = s.SerializeObjectArray<RRRGBA_SaveSlot>(Slots, 2, name: nameof(Slots));
            Flags = s.Serialize<uint>(Flags, name: nameof(Flags));
            Bytes_1B4 = s.SerializeArray<byte>(Bytes_1B4, 6, name: nameof(Bytes_1B4));
            LevelTimes = s.SerializeArray<ushort>(LevelTimes, 28, name: nameof(LevelTimes));
            Bytes_1F2 = s.SerializeArray<byte>(Bytes_1F2, 6, name: nameof(Bytes_1F2));

            p?.Serialize<uint>(s, name: "SaveDataChecksum");
        });
        SelectedSlot = s.Serialize<byte>(SelectedSlot, name: nameof(SelectedSlot));
        s.SerializeMagic<byte>(0xAE);
        s.SerializePadding(2, logIfNotNull: true);
    }
}