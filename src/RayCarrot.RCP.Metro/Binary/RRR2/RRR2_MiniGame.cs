#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_MiniGame : BinarySerializable
{
    public RRR2_ScoreEntry[] Scores { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Scores = s.SerializeObjectArray<RRR2_ScoreEntry>(Scores, 3, name: nameof(Scores));
    }
}