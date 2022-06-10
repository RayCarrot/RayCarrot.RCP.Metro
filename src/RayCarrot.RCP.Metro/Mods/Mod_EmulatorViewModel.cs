namespace RayCarrot.RCP.Metro;

public class Mod_EmulatorViewModel : BaseViewModel
{
    public Mod_EmulatorViewModel(LocalizedString displayName, string[] processNameKeywords, params Mod_MemoryRegion[] memoryRegions)
    {
        DisplayName = displayName;
        ProcessNameKeywords = processNameKeywords;
        MemoryRegions = memoryRegions;
    }

    private const string MainMemoryRegionName = "Main";

    public LocalizedString DisplayName { get; }
    public string[] ProcessNameKeywords { get; }
    public Mod_MemoryRegion MainMemoryRegion => MemoryRegions[0];
    public Mod_MemoryRegion[] MemoryRegions { get; }

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
        memoryRegions: new Mod_MemoryRegion(
            Name: MainMemoryRegionName, 
            GameOffset: 0x00, 
            Length: null,
            ModuleName: null, 
            ProcessOffset: 0x193A1A0,
            IsProcessOffsetAPointer: true));
    public static Mod_EmulatorViewModel DOSBox_0_74_2_1_x86 => new(
        displayName: "DOSBox (0.74-2.1 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        memoryRegions: new Mod_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x194A380,
            IsProcessOffsetAPointer: true));
    public static Mod_EmulatorViewModel DOSBox_0_74_3_x86 => new(
        displayName: "DOSBox (0.74-3 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        memoryRegions: new Mod_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x193C370,
            IsProcessOffsetAPointer: true));
    public static Mod_EmulatorViewModel BizHawk_PS1_2_4_0 => new(
        displayName: "BizHawk Octoshock (2.4.0)",
        processNameKeywords: new[] { "EmuHawk" },
        memoryRegions: new Mod_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x80000000,
            Length: null,
            ModuleName: "octoshock.dll",
            ProcessOffset: 0x0011D880,
            IsProcessOffsetAPointer: false));
    public static Mod_EmulatorViewModel BizHawk_PS1_2_8_0 => new(
        displayName: "BizHawk Octoshock (2.8.0)",
        processNameKeywords: new[] { "EmuHawk" },
        memoryRegions: new Mod_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x80000000,
            Length: null,
            ModuleName: "octoshock.dll",
            ProcessOffset: 0x00317F80,
            IsProcessOffsetAPointer: false));
    public static Mod_EmulatorViewModel VisualBoyAdvance_M_2_1_3 => new(
        displayName: "VisualBoyAdvance-M (2.1.3)",
        processNameKeywords: new[] { "visualboyadvance-m" },
        memoryRegions: new[]
        {
            new Mod_MemoryRegion(
                Name: "WRAM",
                GameOffset: 0x2000000,
                Length: 0x40000,
                ModuleName: null,
                ProcessOffset: 0x014820E4,
                IsProcessOffsetAPointer: true),
            new Mod_MemoryRegion(
                Name: "ROM",
                GameOffset: 0x08000000,
                Length: 0x1000000,
                ModuleName: null,
                ProcessOffset: 0x014820EC,
                IsProcessOffsetAPointer: true),
        });
}

public record Mod_MemoryRegion(string Name, long GameOffset, long? Length, string? ModuleName, long ProcessOffset, bool IsProcessOffsetAPointer);