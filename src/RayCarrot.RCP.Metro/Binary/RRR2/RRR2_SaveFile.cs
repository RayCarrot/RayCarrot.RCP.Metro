#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR2_SaveFile : BinarySerializable
{
    public RRR2_MiniGame[] MiniGames { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        // Each save file contains data for 16 mini-games (even for editions that only have 4)
        MiniGames = s.SerializeObjectArray<RRR2_MiniGame>(MiniGames, 16, name: nameof(MiniGames));
    }
}