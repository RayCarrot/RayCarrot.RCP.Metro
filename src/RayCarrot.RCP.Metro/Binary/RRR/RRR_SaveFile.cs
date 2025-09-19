using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR_SaveFile : BinarySerializable
{
    public RRR_SaveSlot[] StorySlots { get; set; }
    public RRR_SaveSlot ScoreSlot { get; set; }
    public RRR_SaveSlot ConfigSlot { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        StorySlots = s.SerializeObjectArray<RRR_SaveSlot>(StorySlots, 3, name: nameof(StorySlots));
        ScoreSlot = s.SerializeObject<RRR_SaveSlot>(ScoreSlot, name: nameof(ScoreSlot));
        ConfigSlot = s.SerializeObject<RRR_SaveSlot>(ConfigSlot, name: nameof(ConfigSlot));
    }
}