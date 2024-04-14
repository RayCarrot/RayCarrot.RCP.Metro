using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class CPAMemoryData : MemoryData
{
    public GAM_EngineStructure? EngineStructure { get; set; }

    public static Dictionary<string, long> Offsets_R2_PC => new()
    {
        [nameof(EngineStructure)] = 0x500380,
    };

    public static Dictionary<string, long> Offsets_R3_PC => new()
    {
        [nameof(EngineStructure)] = 0x7D7DC0,
    };

    protected override bool ValidateImpl(Context context)
    {
        if (EngineStructure != null &&
            (EngineStructure.EngineMode == GAM_EngineMode.Invalid || !Enum.IsDefined(typeof(GAM_EngineMode), EngineStructure.EngineMode)))
        {
            return false;
        }

        return true;
    }

    protected override void SerializeImpl(Context context)
    {
        EngineStructure = SerializeObject<GAM_EngineStructure>(EngineStructure, nameof(EngineStructure));
    }
}