namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps2DiscProgramLayout : DiscProgramLayout
{
    public Ps2DiscProgramLayout(
        string layoutId,
        string memoryCardCountryCode,
        string memoryCardProductCode,
        ProgramFileSystem fileSystem)
        : base(layoutId, fileSystem)
    {
        MemoryCardCountryCode = memoryCardCountryCode;
        MemoryCardProductCode = memoryCardProductCode;
    }

    public Ps2DiscProgramLayout(
        string layoutId,
        int tracksCount,
        string memoryCardCountryCode,
        string memoryCardProductCode,
        ProgramFileSystem fileSystem)
        : base(layoutId, tracksCount, fileSystem)
    {
        MemoryCardCountryCode = memoryCardCountryCode;
        MemoryCardProductCode = memoryCardProductCode;
    }

    public string MemoryCardCountryCode { get; }
    public string MemoryCardProductCode { get; }
}