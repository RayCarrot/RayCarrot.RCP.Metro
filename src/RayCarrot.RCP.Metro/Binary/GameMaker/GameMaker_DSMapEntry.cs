#nullable disable
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class GameMaker_DSMapEntry : IBinarySerializable
{
    public GameMaker_DSMapDataObject Key { get; set; }
    public GameMaker_DSMapDataObject Value { get; set; }

    public void Serialize(IBinarySerializer s)
    {
        Key = s.SerializeObject<GameMaker_DSMapDataObject>(Key, name: nameof(Key));
        Value = s.SerializeObject<GameMaker_DSMapDataObject>(Value, name: nameof(Value));
    }
}