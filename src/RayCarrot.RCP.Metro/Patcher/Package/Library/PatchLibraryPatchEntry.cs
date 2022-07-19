#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchLibraryPatchEntry : BinarySerializable
{
    public string ID { get; set; }
    public bool IsEnabled { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        ID = s.SerializeString(ID, name: nameof(ID));
        IsEnabled = s.Serialize<bool>(IsEnabled, name: nameof(IsEnabled));
    }
}