namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class SingleFileProgramInstallationStructure : ProgramInstallationStructure
{
    // For now the patcher doesn't support single-file games, but hopefully we
    // can add support for it in the future. Perhaps some sort of delta patches?
    public override bool AllowPatching => false;

    public abstract FileExtension[] SupportedFileExtensions { get; }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        // TODO-UPDATE: Localize
        return new GameLocationValidationResult(location.FilePath.FileExists, "The file does not exist");
    }
}