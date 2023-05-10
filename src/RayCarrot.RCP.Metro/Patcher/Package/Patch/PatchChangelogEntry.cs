#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchChangelogEntry : BinarySerializable
{
    private int Version_Major { get; set; }
    private int Version_Minor { get; set; }
    private int Version_Revision { get; set; }
    public PatchVersion Version
    {
        get => new(Version_Major, Version_Minor, Version_Revision);
        set
        {
            Version_Major = value.Major;
            Version_Minor = value.Minor;
            Version_Revision = value.Revision;
        }
    }

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