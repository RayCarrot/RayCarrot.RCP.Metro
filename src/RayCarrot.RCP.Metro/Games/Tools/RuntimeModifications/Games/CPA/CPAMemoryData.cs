using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

// TODO-UPDATE: Add more data
public class CPAMemoryData : MemoryData
{
    public string? CurrentMap { get; set; }
    public byte EngineMode { get; set; }

    public static Dictionary<string, long> Offsets_R2_PC => new()
    {
        [nameof(CurrentMap)] = 0x500380 + 0x1F,
        [nameof(EngineMode)] = 0x500380 + 0x00,
    };

    public static Dictionary<string, long> Offsets_R3_PC => new()
    {
        [nameof(CurrentMap)] = 0x7D7DC0 + 0x1F,
        [nameof(EngineMode)] = 0x7D7DC0 + 0x00,
    };

    protected override bool ValidateImpl(Context context)
    {
        // TODO: Implement some way of validating values. Maybe we need to read more values to properly do so?
        return true;
    }

    protected override void SerializeImpl(Context context)
    {
        CurrentMap = SerializeString(CurrentMap, 16, name: nameof(CurrentMap));
        EngineMode = Serialize<byte>(EngineMode, name: nameof(EngineMode));
    }
}