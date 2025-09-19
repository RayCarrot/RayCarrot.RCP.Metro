using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_ScoreEntry : BinarySerializable
{
    public ulong EncodedName { get; set; }
    public uint Uint_08 { get; set; }
    public int Score { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        // Name is stored as two 32-bit little-endian integers; for simplicity treat it as a single 64-bit integer
        EncodedName = s.Serialize<ulong>(EncodedName, name: nameof(EncodedName));
        Uint_08 = s.Serialize<uint>(Uint_08, name: nameof(Uint_08));
        Score = s.Serialize<int>(Score, name: nameof(Score));
    }
}