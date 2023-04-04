namespace RayCarrot.RCP.Metro.Games.Structure;

public class ExeProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".exe"),
    };
}