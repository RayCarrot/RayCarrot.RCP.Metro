using System.Diagnostics;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class EmulatorManager
{
    #region Constructor

    public EmulatorManager(
        LocalizedString displayName, 
        params MemoryRegion[] memoryRegions)
    {
        DisplayName = displayName;
        MemoryRegions = memoryRegions;
    }

    #endregion

    #region Constants

    public const string MainMemoryRegionName = "Main";

    #endregion

    #region Public Properties

    public LocalizedString DisplayName { get; }
    public MemoryRegion[] MemoryRegions { get; }

    #endregion

    #region Public Methods

    public virtual bool IsProcessValid(Process process) => true;

    #endregion

    #region Platforms

    public static EmulatorManager[] None => new[]
    {
        new EmulatorManager(
            displayName: new ResourceLocString(nameof(Resources.EmuSelection_None)),
            memoryRegions: new MemoryRegion(
                Name: MainMemoryRegionName,
                GameOffset: 0x00,
                Length: null,
                ModuleName: null,
                ProcessOffset: 0x00,
                IsProcessOffsetAPointer: false)),
    };
    public static EmulatorManager[] MsDos => new EmulatorManager[] { DosBox_0_74_3_x86, DosBox_0_74_2_x86, DosBox_0_74_x86, };
    public static EmulatorManager[] Ps1 => new EmulatorManager[] { BizHawk_Ps1_2_8_0_x64, BizHawk_Ps1_2_4_0_x64, };
    public static EmulatorManager[] Gba => new EmulatorManager[] { VisualBoyAdvance_M_2_1_9_x86, VisualBoyAdvance_M_2_1_3_x86 };

    #endregion

    #region DOSBox

    // A mostly consistent way of finding the DOSBox game pointer (there probably is a better way):
    // - Search for "RAY1.WLD" in cheat engine while in a Jungle level. The first result should be correct, but you can go to a Music
    //   level to verify it changes to "RAY2.WLD".
    // - Check what writes to the value by right-clicking it. Change world and the list should get populated.
    // - The first one has the EAX set to the current game base address and the ECX to the string offset.
    // - Search for the EAX value. The last result should be the static pointer to it.
    public static DosBoxEmulatorManager DosBox_0_74_x86 => new(
        displayName: "DOSBox (0.74 - x86)",
        majorVersion: 0, 
        minorVersion: 74, 
        buildVersion: 0, 
        is64Bit: false,
        memoryRegions: new MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x1D3A1A0,
            IsProcessOffsetAPointer: true));
    public static DosBoxEmulatorManager DosBox_0_74_2_x86 => new(
        displayName: "DOSBox (0.74-2 - x86)",
        majorVersion: 0,
        minorVersion: 74,
        buildVersion: 2,
        is64Bit: false,
        memoryRegions: new MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x1D4A380,
            IsProcessOffsetAPointer: true));
    public static DosBoxEmulatorManager DosBox_0_74_3_x86 => new(
        displayName: "DOSBox (0.74-3 - x86)",
        majorVersion: 0,
        minorVersion: 74,
        buildVersion: 3,
        is64Bit: false,
        memoryRegions: new MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x00,
            Length: null,
            ModuleName: null,
            ProcessOffset: 0x1D3C370,
            IsProcessOffsetAPointer: true));

    #endregion

    #region BizHawk

    // TODO-UPDATE: Implement process check
    public static EmulatorManager BizHawk_Ps1_2_4_0_x64 => new(
        displayName: "BizHawk Octoshock (2.4.0 - x64)",
        memoryRegions: new MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x80000000,
            Length: null,
            ModuleName: "octoshock.dll",
            ProcessOffset: 0x0011D880,
            IsProcessOffsetAPointer: false));
    public static EmulatorManager BizHawk_Ps1_2_8_0_x64 => new(
        displayName: "BizHawk Octoshock (2.8.0 - x64)",
        memoryRegions: new MemoryRegion(
            Name: MainMemoryRegionName,
            GameOffset: 0x80000000,
            Length: null,
            ModuleName: "octoshock.dll",
            ProcessOffset: 0x00317F80,
            IsProcessOffsetAPointer: false));

    #endregion

    #region VisualBoyAdvance-M

    // TODO-UPDATE: Implement process check
    public static EmulatorManager VisualBoyAdvance_M_2_1_3_x86 => new(
        displayName: "VisualBoyAdvance-M (2.1.3 - x86)",
        memoryRegions: new[]
        {
            new MemoryRegion(
                Name: "WRAM",
                GameOffset: 0x2000000,
                Length: 0x40000,
                ModuleName: null,
                ProcessOffset: 0x018820E4,
                IsProcessOffsetAPointer: true),
            //new MemoryRegion(
            //    Name: "IRAM",
            //    GameOffset: 0x3000000,
            //    Length: 0x8000,
            //    ModuleName: null,
            //    ProcessOffset: 0x018820E8,
            //    IsProcessOffsetAPointer: true),
            new MemoryRegion(
                Name: "ROM",
                GameOffset: 0x08000000,
                Length: 0x1000000,
                ModuleName: null,
                ProcessOffset: 0x018820EC,
                IsProcessOffsetAPointer: true),
        });
    public static EmulatorManager VisualBoyAdvance_M_2_1_9_x86 => new(
        displayName: "VisualBoyAdvance-M (2.1.9 - x86)",
        memoryRegions: new[]
        {
            new MemoryRegion(
                Name: "WRAM",
                GameOffset: 0x2000000,
                Length: 0x40000,
                ModuleName: null,
                ProcessOffset: 0x03FEADEC,
                IsProcessOffsetAPointer: true),
            new MemoryRegion(
                Name: "ROM",
                GameOffset: 0x08000000,
                Length: 0x1000000,
                ModuleName: null,
                ProcessOffset: 0x03FEA97C,
                IsProcessOffsetAPointer: true),
        });

    #endregion
}