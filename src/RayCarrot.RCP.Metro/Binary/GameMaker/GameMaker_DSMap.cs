#nullable disable
using System.Linq;
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class GameMaker_DSMap : IBinarySerializable
{
    public int Magic { get; set; } // 402
    public int EntriesCount { get; set; }
    public GameMaker_DSMapEntry[] Entries { get; set; }

    public GameMaker_DSMapDataObject GetValue(string key) => Entries.First(x => x.Key.StringValue == key).Value;

    public void Serialize(IBinarySerializer s)
    {
        Magic = s.Serialize<int>(Magic, name: nameof(Magic));

        if (Magic != 402)
            throw new BinarySerializableException($"Invalid magic {Magic}. Expected 402.");

        EntriesCount = s.Serialize<int>(EntriesCount, name: nameof(EntriesCount));
        Entries = s.SerializeObjectArray<GameMaker_DSMapEntry>(Entries, EntriesCount, name: nameof(Entries));
    }
}