using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Rayman2Ps2SaveInfo : BinarySerializable
{
    public string Description { get; set; } // Stage X/10
    public string Name { get; set; }
    public uint Time { get; set; }
    public ushort Percentage { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Description = s.SerializeString(Description, length: 32, name: nameof(Description));
        Name = s.SerializeString(Name, length: 32, name: nameof(Name));
        Time = s.Serialize<uint>(Time, name: nameof(Time));
        Percentage = s.Serialize<ushort>(Percentage, name: nameof(Percentage));
        s.SerializePadding(2, logIfNotNull: true);
    }
}