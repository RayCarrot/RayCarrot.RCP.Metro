namespace RayCarrot.RCP.Metro;

public class Mod_Mem_EmulatorViewModel : BaseViewModel
{
    #region Constructor

    public Mod_Mem_EmulatorViewModel(LocalizedString displayName, string[] processNameKeywords, params Mod_Mem_MemoryRegion[] memoryRegions)
    {
        DisplayName = displayName;
        ProcessNameKeywords = processNameKeywords;
        MemoryRegions = memoryRegions;
    }

    #endregion

    #region Constants

    private const string MainMemoryRegionName = "Main";

    #endregion

    #region Public Properties

    public LocalizedString DisplayName { get; }
    public string[] ProcessNameKeywords { get; }
    public Mod_Mem_MemoryRegion MainMemoryRegion => MemoryRegions[0];
    public Mod_Mem_MemoryRegion[] MemoryRegions { get; }

    #endregion

    #region Platforms

    public static Mod_Mem_EmulatorViewModel[] None => new[]
    {
        new Mod_Mem_EmulatorViewModel(
            displayName: new ResourceLocString(nameof(Resources.EmuSelection_None)),
            processNameKeywords: Array.Empty<string>(),
            memoryRegions: new Mod_Mem_MemoryRegion(
                Name: MainMemoryRegionName,
                GameOffset: 0x00,
                Length: null,
                ModuleName: null,
                ProcessOffset: 0x00,
                IsProcessOffsetAPointer: false)),
    };
    public static Mod_Mem_EmulatorViewModel[] MSDOS => new[] { DOSBox_0_74_3_x86, DOSBox_0_74_2_1_x86, DOSBox_0_74_x86,  };
    public static Mod_Mem_EmulatorViewModel[] PS1 => new[] { BizHawk_PS1_2_8_0_x64, BizHawk_PS1_2_4_0_x64, };
    public static Mod_Mem_EmulatorViewModel[] GBA => new[] { VisualBoyAdvance_M_2_1_3_x86 };

    #endregion

    #region DOSBox

    // A mostly consistent way of finding the DOSBox game pointer (there probably is a better way):
    // - Search for "RAY1.WLD" in cheat engine while in a Jungle level. The first result should be correct, but you can go to a Music
    //   level to verify it changes to "RAY2.WLD".
    // - Check what writes to the value by right-clicking it. Change world and the list should get populated.
    // - The first one has the EAX set to the current game base address and the ECX to the string offset.
    // - Search for the EAX value. The last result should be the static pointer to it.
    public static Mod_Mem_EmulatorViewModel DOSBox_0_74_x86 => new(
        displayName: "DOSBox (0.74 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        memoryRegions: new Mod_Mem_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x1D3A1A0,
            IsProcessOffsetAPointer: true));
    public static Mod_Mem_EmulatorViewModel DOSBox_0_74_2_1_x86 => new(
        displayName: "DOSBox (0.74-2.1 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        memoryRegions: new Mod_Mem_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x1D4A380,
            IsProcessOffsetAPointer: true));
    public static Mod_Mem_EmulatorViewModel DOSBox_0_74_3_x86 => new(
        displayName: "DOSBox (0.74-3 - x86)",
        processNameKeywords: new[] { "DOSBox" },
        memoryRegions: new Mod_Mem_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x1D3C370,
            IsProcessOffsetAPointer: true));

    #endregion

    #region BizHawk

    public static Mod_Mem_EmulatorViewModel BizHawk_PS1_2_4_0_x64 => new(
        displayName: "BizHawk Octoshock (2.4.0 - x64)",
        processNameKeywords: new[] { "EmuHawk" },
        memoryRegions: new Mod_Mem_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x80000000,
            Length: null,
            ModuleName: "octoshock.dll",
            ProcessOffset: 0x0011D880,
            IsProcessOffsetAPointer: false));
    public static Mod_Mem_EmulatorViewModel BizHawk_PS1_2_8_0_x64 => new(
        displayName: "BizHawk Octoshock (2.8.0 - x64)",
        processNameKeywords: new[] { "EmuHawk" },
        memoryRegions: new Mod_Mem_MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x80000000,
            Length: null,
            ModuleName: "octoshock.dll",
            ProcessOffset: 0x00317F80,
            IsProcessOffsetAPointer: false));

    #endregion

    #region VisualBoyAdvance-M

    public static Mod_Mem_EmulatorViewModel VisualBoyAdvance_M_2_1_3_x86 => new(
        displayName: "VisualBoyAdvance-M (2.1.3 - x86)",
        processNameKeywords: new[] { "visualboyadvance-m" },
        memoryRegions: new[]
        {
            new Mod_Mem_MemoryRegion(
                Name: "WRAM",
                GameOffset: 0x2000000,
                Length: 0x40000,
                ModuleName: null,
                ProcessOffset: 0x018820E4,
                IsProcessOffsetAPointer: true),
            new Mod_Mem_MemoryRegion(
                Name: "ROM",
                GameOffset: 0x08000000,
                Length: 0x1000000,
                ModuleName: null,
                ProcessOffset: 0x018820EC,
                IsProcessOffsetAPointer: true),
        });

    #endregion
}