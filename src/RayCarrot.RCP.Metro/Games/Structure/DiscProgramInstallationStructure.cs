namespace RayCarrot.RCP.Metro.Games.Structure;

public class DiscProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public DiscProgramInstallationStructure(DiscProgramLayout[] layouts) : base(layouts)
    {
        Layouts = layouts;
    }

    public new DiscProgramLayout[] Layouts { get; }

    public override bool SupportGameFileFinder => true;

    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".bin"),
        new FileExtension(".cue"),
        new FileExtension(".iso"),
    };
}