namespace RayCarrot.RCP.Metro.Games.Structure;

public class DiscProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".bin"),
        new FileExtension(".cue"),
        new FileExtension(".iso"),
    };
}