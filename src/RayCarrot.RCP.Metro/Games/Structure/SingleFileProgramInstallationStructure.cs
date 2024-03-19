using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class SingleFileProgramInstallationStructure : ProgramInstallationStructure
{
    public virtual bool SupportGameFileFinder => false;

    public abstract FileExtension[] SupportedFileExtensions { get; }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Single-file games need to store the mod library in a folder based on its file name to avoid conflicts
        // if there are multiple single-file games in the same folder
        builder.Register(new ModLibraryPathComponent(x => x.InstallLocation.Directory + $".rcp_mods_{x.InstallLocation.FileName}"));
    }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        return new GameLocationValidationResult(location.FilePath.FileExists, Resources.Games_ValidationFileMissing);
    }
}