namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbaProgramLayout : ProgramLayout
{
    public GbaProgramLayout(string layoutId, string gameTitle, string gameCode, string makerCode) : base(layoutId)
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