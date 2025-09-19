#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class GAM_EngineStructure : BinarySerializable
{
    public GAM_EngineMode EngineMode { get; set; }
    public string LevelName { get; set; }
    public string NextLevelName { get; set; }
    public string FirstLevelName { get; set; }

    public IPT_InputMode InputMode { get; set; }
    public GAM_DisplayFixMode DisplayFixMode { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        EngineMode = s.Serialize<GAM_EngineMode>(EngineMode, name: nameof(EngineMode));
        LevelName = s.SerializeString(LevelName, length: 30, name: nameof(LevelName));
        NextLevelName = s.SerializeString(NextLevelName, length: 30, name: nameof(NextLevelName));
        FirstLevelName = s.SerializeString(FirstLevelName, length: 30, name: nameof(FirstLevelName));
        InputMode = s.Serialize<IPT_InputMode>(InputMode, name: nameof(InputMode));
        DisplayFixMode = s.Serialize<GAM_DisplayFixMode>(DisplayFixMode, name: nameof(DisplayFixMode));

        // NOTE: There is more to this struct, but we don't need the rest right now
    }
}