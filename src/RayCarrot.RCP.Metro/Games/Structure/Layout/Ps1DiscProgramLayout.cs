namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps1DiscProgramLayout : DiscProgramLayout
{
    public Ps1DiscProgramLayout(
        string layoutId,
        string countryCode,
        string productCode, 
        ProgramFileSystem fileSystem) 
        : base(layoutId, fileSystem)
    {
        CountryCode = countryCode;
        ProductCode = productCode;
    }

    public Ps1DiscProgramLayout(
        string layoutId,
        string countryCode,
        string productCode, 
        int tracksCount, 
        ProgramFileSystem fileSystem) 
        : base(layoutId, tracksCount, fileSystem)
    {
        CountryCode = countryCode;
        ProductCode = productCode;
    }

    public string CountryCode { get; }
    public string ProductCode { get; }
}