namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps2DiscProgramLayout : DiscProgramLayout
{
    public Ps2DiscProgramLayout(
        string layoutId,
        ProgramFileSystem fileSystem) 
        : base(layoutId, fileSystem) { }

    public Ps2DiscProgramLayout(
        string layoutId,
        int tracksCount, 
        ProgramFileSystem fileSystem) 
        : base(layoutId, tracksCount, fileSystem) { }
}