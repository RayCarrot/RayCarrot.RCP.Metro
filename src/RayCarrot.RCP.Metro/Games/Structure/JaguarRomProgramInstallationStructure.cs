namespace RayCarrot.RCP.Metro.Games.Structure;

public class JaguarRomProgramInstallationStructure : RomProgramInstallationStructure
{
    public override bool SupportGameFileFinder => false;
    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".jag"),
        new FileExtension(".j64"),
        new FileExtension(".rom"),
    };
}