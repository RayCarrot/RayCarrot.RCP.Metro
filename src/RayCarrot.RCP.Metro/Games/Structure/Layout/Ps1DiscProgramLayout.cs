namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps1DiscProgramLayout : DiscProgramLayout
{
    public Ps1DiscProgramLayout(
        string layoutId,
        string memoryCardCountryCode,
        string memoryCardProductCode, 
        ProgramFileSystem fileSystem) 
        : base(layoutId, fileSystem)
    {
        MemoryCardCountryCode = memoryCardCountryCode;
        MemoryCardProductCode = memoryCardProductCode;
    }

    public Ps1DiscProgramLayout(
        string layoutId,
        string memoryCardCountryCode,
        string memoryCardProductCode, 
        int tracksCount, 
        ProgramFileSystem fileSystem) 
        : base(layoutId, tracksCount, fileSystem)
    {
        MemoryCardCountryCode = memoryCardCountryCode;
        MemoryCardProductCode = memoryCardProductCode;
    }

    public string MemoryCardCountryCode { get; }
    public string MemoryCardProductCode { get; }
}