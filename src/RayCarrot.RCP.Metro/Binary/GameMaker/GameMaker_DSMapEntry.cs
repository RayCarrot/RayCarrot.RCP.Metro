#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class GameMaker_DSMapEntry : BinarySerializable
{
    public GameMaker_DSMapDataObject Key { get; set; }
    public GameMaker_DSMapDataObject Value { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Key = s.SerializeObject<GameMaker_DSMapDataObject>(Key, name: nameof(Key));
        Value = s.SerializeObject<GameMaker_DSMapDataObject>(Value, name: nameof(Value));
    }
}