namespace RayCarrot.RCP.Metro.Games.Structure;

public class DiscProgramLayout : ProgramLayout
{
    public DiscProgramLayout(string variantId, ProgramFileSystem fileSystem) : base(variantId)
    {
        FileSystem = fileSystem;
    }

    public ProgramFileSystem FileSystem { get; }
}