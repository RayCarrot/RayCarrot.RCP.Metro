#nullable disable
using BinarySerializer;
using System.Text;
namespace RayCarrot.RCP.Metro;

public class RRR2_ScoreEntry : BinarySerializable
{
    public string Name { get; set; }
    public int Score { get; set; }
    public override void SerializeImpl(SerializerObject s)
    {
        byte[] nameBytes = new byte[8];
        nameBytes = s.SerializeArray<byte>(nameBytes, 8, name: nameof(nameBytes));
        StringBuilder sb = new();
        sb.Append((char)nameBytes[3]);
        sb.Append((char)nameBytes[2]);
        sb.Append((char)nameBytes[1]);
        sb.Append((char)nameBytes[0]);
        sb.Append((char)nameBytes[7]);
        sb.Append((char)nameBytes[6]);
        sb.Append((char)nameBytes[5]);
        sb.Append((char)nameBytes[4]);

        Name = sb.ToString().TrimEnd('\0');

        s.SerializePadding(4);
        Score = s.Serialize<int>(Score, name: nameof(Score));
    }
}