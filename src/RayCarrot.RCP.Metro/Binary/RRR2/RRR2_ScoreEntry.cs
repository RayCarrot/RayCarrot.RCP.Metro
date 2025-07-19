#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_ScoreEntry : BinarySerializable
{
    public string Name { get; set; }
    public int Score { get; set; }
    public override void SerializeImpl(SerializerObject s)
    {
        Name = s.SerializeString(Name, length: 8, name: nameof(Name));
        s.SerializePadding(4);
        Score = s.Serialize<int>(Score, name: nameof(Score));
    }
}