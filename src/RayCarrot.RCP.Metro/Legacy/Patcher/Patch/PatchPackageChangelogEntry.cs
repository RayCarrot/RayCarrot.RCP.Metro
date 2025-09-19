using BinarySerializer;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

public class PatchPackageChangelogEntry : BinarySerializable
{
    public int Version_Major { get; set; }
    public int Version_Minor { get; set; }
    public int Version_Revision { get; set; }

    private long DateValue { get; set; }
    public DateTime Date
    {
        get => DateTime.FromBinary(DateValue);
        set => DateValue = value.ToBinary();
    }

    public string Description { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Version_Major = s.Serialize<int>(Version_Major, name: nameof(Version_Major));
        Version_Minor = s.Serialize<int>(Version_Minor, name: nameof(Version_Minor));
        Version_Revision = s.Serialize<int>(Version_Revision, name: nameof(Version_Revision));

        DateValue = s.Serialize<long>(DateValue, name: nameof(DateValue));
        s.Log($"Date: {Date}");

        Description = s.SerializeString(Description, name: nameof(Description));
    }
}