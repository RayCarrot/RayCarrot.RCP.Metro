namespace RayCarrot.RCP.Metro.Games.Structure;

public class DiscProgramLayout : ProgramLayout
{
    public DiscProgramLayout(string variantId, ProgramFileSystem fileSystem) : base(variantId)
    {
        TracksCount = 1;
        FileSystem = fileSystem;
    }

    public DiscProgramLayout(string variantId, int tracksCount, ProgramFileSystem fileSystem) : base(variantId)
    {
        TracksCount = tracksCount;
        FileSystem = fileSystem;
    }

    public ProgramFileSystem FileSystem { get; }
    public int TracksCount { get; }
}