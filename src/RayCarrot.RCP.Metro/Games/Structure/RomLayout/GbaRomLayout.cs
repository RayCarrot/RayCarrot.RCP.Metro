namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbaRomLayout : RomLayout
{
    public GbaRomLayout(string variantId, string gameTitle, string gameCode, string makerCode) : base(variantId)
    {
        GameTitle = gameTitle;
        GameCode = gameCode;
        MakerCode = makerCode;
    }

    // These properties match to the same properties in the ROM header
    public string GameTitle { get; }
    public string GameCode { get; }
    public string MakerCode { get; }
}