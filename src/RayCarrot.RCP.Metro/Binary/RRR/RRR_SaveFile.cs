#nullable disable
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class RRR_SaveFile : IBinarySerializable
{
    public RRR_SaveSlot[] StorySlots { get; set; }
    public RRR_SaveSlot ScoreSlot { get; set; }
    public RRR_SaveSlot ConfigSlot { get; set; }

    public void Serialize(IBinarySerializer s)
    {
        StorySlots = s.SerializeObjectArray<RRR_SaveSlot>(StorySlots, 3, name: nameof(StorySlots));
        ScoreSlot = s.SerializeObject<RRR_SaveSlot>(ScoreSlot, name: nameof(ScoreSlot));
        ConfigSlot = s.SerializeObject<RRR_SaveSlot>(ConfigSlot, name: nameof(ConfigSlot));
    }
}