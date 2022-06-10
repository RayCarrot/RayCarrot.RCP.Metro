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

    // A mostly consistent way of finding the DOSBox game pointer (there probably is a better way):
    // - Search for "RAY1.WLD" in cheat engine while in a Jungle level. The first result should be correct, but you can go to a Music
    //   level to verify it changes to "RAY2.WLD".
    // - Check what writes to the value by right-clicking it. Change world and the list should get populated.
    // - The first one has the EAX set to the current game base address and the ECX to the string offset.
    // - Search for the EAX value. The last result should be the static pointer to it.
    // TODO-UPDATE: Localize
    public static Mod_EmulatorViewModel DOSBox_0_74_x86 => new(
        displayName: "DOSBox (0.74 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        moduleName: null,
        gameBaseOffset: 0x01D3A1A0,
        isGameBaseAPointer: true);
    public static Mod_EmulatorViewModel DOSBox_0_74_2_1_x86 => new(
        displayName: "DOSBox (0.74-2.1 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        moduleName: null,
        gameBaseOffset: 0x1D4A380,
        isGameBaseAPointer: true);
    public static Mod_EmulatorViewModel DOSBox_0_74_3_x86 => new(
        displayName: "DOSBox (0.74-3 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        moduleName: null,
        gameBaseOffset: 0x1D3C370,
        isGameBaseAPointer: true);
    public static Mod_EmulatorViewModel BizHawk_PS1_2_4_0 => new(
        displayName: "BizHawk Octoshock (2.4.0)",
        processNameKeywords: new[] { "EmuHawk" },
        moduleName: "octoshock.dll",
        gameBaseOffset: 0x0011D880 - (long)0x80000000,
        isGameBaseAPointer: false);
    public static Mod_EmulatorViewModel BizHawk_PS1_2_8_0 => new(
        displayName: "BizHawk Octoshock (2.8.0)",
        processNameKeywords: new[] { "EmuHawk" },
        moduleName: "octoshock.dll",
        gameBaseOffset: 0x00317F80 - (long)0x80000000,
        isGameBaseAPointer: false);
}