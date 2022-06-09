namespace RayCarrot.RCP.Metro;

public class Mod_EmulatorViewModel : BaseViewModel
{
    public Mod_EmulatorViewModel(LocalizedString displayName, string[] processNameKeywords, string? moduleName, long gameBaseOffset, bool isGameBaseAPointer)
    {
        DisplayName = displayName;
        ProcessNameKeywords = processNameKeywords;
        ModuleName = moduleName;
        GameBaseOffset = gameBaseOffset;
        IsGameBaseAPointer = isGameBaseAPointer;
    }

    public LocalizedString DisplayName { get; }
    public string[] ProcessNameKeywords { get; }
    public string? ModuleName { get; }
    public long GameBaseOffset { get; }
    public bool IsGameBaseAPointer { get; }

    // TODO-UPDATE: Localize
    public static Mod_EmulatorViewModel DOSBox_0_74 => new(
        displayName: "DOSBox (0.74)",
        processNameKeywords: new[] { "DOSBox" },
        moduleName: null,
        gameBaseOffset: 0x01D3A1A0,
        isGameBaseAPointer: true);
    public static Mod_EmulatorViewModel BizHawk_PS1_2_4_0 => new(
        displayName: "BizHawk (PS1 - 2.4.0)",
        processNameKeywords: new[] { "EmuHawk" },
        moduleName: "octoshock.dll",
        gameBaseOffset: 0x0011D880 - (long)0x80000000,
        isGameBaseAPointer: false);
    public static Mod_EmulatorViewModel BizHawk_PS1_2_8_0 => new(
        displayName: "BizHawk (PS1 - 2.8.0)",
        processNameKeywords: new[] { "EmuHawk" },
        moduleName: "octoshock.dll",
        gameBaseOffset: 0x00317F80 - (long)0x80000000,
        isGameBaseAPointer: false);
}