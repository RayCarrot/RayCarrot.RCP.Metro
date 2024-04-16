namespace RayCarrot.RCP.Metro.Games.Structure;

public class DiscProgramLayout : ProgramLayout
{
    public DiscProgramLayout(string layoutId, ProgramFileSystem fileSystem) : base(layoutId)
    {
        TracksCount = 1;
        FileSystem = fileSystem;
    }

    public DiscProgramLayout(string layoutId, int tracksCount, ProgramFileSystem fileSystem) : base(layoutId)
    {
        TracksCount = tracksCount;
        FileSystem = fileSystem;
    }

    public ProgramFileSystem FileSystem { get; }
    public int TracksCount { get; }
}