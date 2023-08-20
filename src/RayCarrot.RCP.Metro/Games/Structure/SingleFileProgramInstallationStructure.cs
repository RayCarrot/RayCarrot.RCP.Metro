namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class SingleFileProgramInstallationStructure : ProgramInstallationStructure
{
    // For now the mod loader doesn't support single-file games, but hopefully we
    // can add support for it in the future. Perhaps some sort of delta patches?
    public override bool SupportsMods => false;

    public virtual bool SupportGameFileFinder => false;

    public abstract FileExtension[] SupportedFileExtensions { get; }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        return new GameLocationValidationResult(location.FilePath.FileExists, Resources.Games_ValidationFileMissing);
    }
}