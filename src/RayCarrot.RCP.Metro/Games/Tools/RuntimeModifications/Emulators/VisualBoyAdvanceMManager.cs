using System.Diagnostics;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class VisualBoyAdvanceMManager : EmulatorManager
{
    public VisualBoyAdvanceMManager(
        LocalizedString displayName, 
        int majorVersion, 
        int minorVersion, 
        int buildVersion, 
        bool is64Bit,
        params MemoryRegion[] memoryRegions) 
        : base(displayName, memoryRegions)
    {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        BuildVersion = buildVersion;
        Is64Bit = is64Bit;
    }

    public int MajorVersion { get; }
    public int MinorVersion { get; }
    public int BuildVersion { get; }
    public bool Is64Bit { get; }

    public override bool IsProcessValid(Process process)
    {
        if (process.Is64Bit() != Is64Bit)
            return false;

        ProcessModule? module = process.MainModule;

        if (module == null)
            return false;

        FileVersionInfo info = module.FileVersionInfo;

        return info.ProductMajorPart == MajorVersion &&
               info.ProductMinorPart == MinorVersion &&
               info.ProductBuildPart == BuildVersion &&
               info.ProductName.Contains("VisualBoyAdvance-M");
    }
}