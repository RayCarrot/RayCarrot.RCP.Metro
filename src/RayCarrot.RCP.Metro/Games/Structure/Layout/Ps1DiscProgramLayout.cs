namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps1DiscProgramLayout : DiscProgramLayout
{
    public Ps1DiscProgramLayout(
        string variantId,
        string memoryCardCountryCode,
        string memoryCardProductCode, 
        ProgramFileSystem fileSystem) 
        : base(variantId, fileSystem)
    {
        MemoryCardCountryCode = memoryCardCountryCode;
        MemoryCardProductCode = memoryCardProductCode;
    }

    public Ps1DiscProgramLayout(
        string variantId,
        string memoryCardCountryCode,
        string memoryCardProductCode, 
        int tracksCount, 
        ProgramFileSystem fileSystem) 
        : base(variantId, tracksCount, fileSystem)
    {
        MemoryCardCountryCode = memoryCardCountryCode;
        MemoryCardProductCode = memoryCardProductCode;
    }

    public string MemoryCardCountryCode { get; }
    public string MemoryCardProductCode { get; }
}