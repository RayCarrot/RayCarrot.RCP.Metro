namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbcProgramLayout : ProgramLayout
{
    public GbcProgramLayout(string layoutId, string gameTitle, string manufacturerCode, string licenseeCode) : base(layoutId)
    {
        GameTitle = gameTitle;
        ManufacturerCode = manufacturerCode;
        LicenseeCode = licenseeCode;
    }

    // These properties match to the same properties in the ROM header
    public string GameTitle { get; }
    public string ManufacturerCode { get; }
    public string LicenseeCode { get; }
}