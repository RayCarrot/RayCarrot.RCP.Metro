namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps2DiscProgramLayout : DiscProgramLayout
{
    public Ps2DiscProgramLayout(
        string layoutId,
        string countryCode,
        string productCode,
        ProgramFileSystem fileSystem)
        : base(layoutId, fileSystem)
    {
        CountryCode = countryCode;
        ProductCode = productCode;
    }

    public Ps2DiscProgramLayout(
        string layoutId,
        int tracksCount,
        string countryCode,
        string productCode,
        ProgramFileSystem fileSystem)
        : base(layoutId, tracksCount, fileSystem)
    {
        CountryCode = countryCode;
        ProductCode = productCode;
    }

    public string CountryCode { get; }
    public string ProductCode { get; }
}