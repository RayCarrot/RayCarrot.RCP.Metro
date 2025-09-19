#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

public class DeltaChunk : BinarySerializable
{
    public long PatchOffset { get; set; }
    public byte[] PatchData { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        PatchOffset = s.Serialize<long>(PatchOffset, name: nameof(PatchOffset));
        PatchData = s.SerializeArraySize<byte, long>(PatchData, name: nameof(PatchData));
        PatchData = s.SerializeArray<byte>(PatchData, PatchData.Length, name: nameof(PatchData));
    }
}