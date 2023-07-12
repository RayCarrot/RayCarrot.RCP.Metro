namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbcRomLayout : RomLayout
{
    public GbcRomLayout(string variantId, string gameTitle, string manufacturerCode, string licenseeCode) : base(variantId)
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