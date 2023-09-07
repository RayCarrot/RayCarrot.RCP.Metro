namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class SingleFileProgramInstallationStructure : ProgramInstallationStructure
{
    public virtual bool SupportGameFileFinder => false;

    public abstract FileExtension[] SupportedFileExtensions { get; }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        return new GameLocationValidationResult(location.FilePath.FileExists, Resources.Games_ValidationFileMissing);
    }
}