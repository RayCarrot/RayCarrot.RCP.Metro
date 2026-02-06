namespace RayCarrot.RCP.Metro.Games.Structure;

public class GameCubeProgramLayout : ProgramLayout
{
    public GameCubeProgramLayout(string layoutId, string gameTitle, string gameCode, string makerCode) : base(layoutId)
    {
        GameTitle = gameTitle;
        GameCode = gameCode;
        MakerCode = makerCode;
    }

    // These properties match to the same properties in the disc header
    public string GameTitle { get; }
    public string GameCode { get; }
    public string MakerCode { get; }
}