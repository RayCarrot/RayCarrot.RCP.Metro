#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RHR_LevelSave : BinarySerializable
{
    public ushort Score { get; set; }
    public byte Lums { get; set; }
    public RHR_LevelSaveFlags Flags { get; set; }
    public byte Teensies { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Score = s.Serialize<ushort>(Score, name: nameof(Score));
        Lums = s.Serialize<byte>(Lums, name: nameof(Lums));

        s.DoBits<byte>(b =>
        {
            Flags = b.SerializeBits<RHR_LevelSaveFlags>(Flags, 5, name: nameof(Flags));
            Teensies = b.SerializeBits<byte>(Teensies, 3, name: nameof(Teensies));
        });
    }
}