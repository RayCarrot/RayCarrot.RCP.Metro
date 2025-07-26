#nullable disable
using BinarySerializer;
using System.Text;
namespace RayCarrot.RCP.Metro;

public class RRR2_ScoreEntry : BinarySerializable
{
    public ulong EncodedName { get; set; }
    public int Score { get; set; }
    public override void SerializeImpl(SerializerObject s)
    {
        EncodedName = s.Serialize<ulong>(EncodedName, name: nameof(EncodedName));
        s.SerializePadding(4);
        Score = s.Serialize<int>(Score, name: nameof(Score));
    }
}