#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_SaveFile : BinarySerializable
{
    public RRR2_MiniGame[] MiniGames { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        MiniGames = s.SerializeObjectArray<RRR2_MiniGame>(MiniGames, 16, name: nameof(MiniGames));
    }
}