using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RHR_Highscore : BinarySerializable
{
    public int Score { get; set; }
    public short Lums { get; set; }
    public short Teensies { get; set; }
    public string Name { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Score = s.Serialize<int>(Score, name: nameof(Score));
        Lums = s.Serialize<short>(Lums, name: nameof(Lums));
        Teensies = s.Serialize<short>(Teensies, name: nameof(Teensies));
        Name = s.SerializeString(Name, 8, name: nameof(Name));
    }
}