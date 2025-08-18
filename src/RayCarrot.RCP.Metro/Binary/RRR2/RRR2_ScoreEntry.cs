#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_ScoreEntry : BinarySerializable
{
    public ulong EncodedName { get; set; }
    public int Score { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        // Name is stored as two 32-bit little-endian integers; for simplicity treat it as a single 64-bit integer
        EncodedName = s.Serialize<ulong>(EncodedName, name: nameof(EncodedName));
        s.SerializePadding(4);
        Score = s.Serialize<int>(Score, name: nameof(Score));
    }
}