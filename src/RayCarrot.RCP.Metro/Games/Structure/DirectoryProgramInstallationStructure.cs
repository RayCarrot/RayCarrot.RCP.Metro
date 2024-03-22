using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;
using RayCarrot.RCP.Metro.ModLoader.Modules.Files;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class DirectoryProgramInstallationStructure : ProgramInstallationStructure
{
    public DirectoryProgramInstallationStructure(ProgramFileSystem fileSystem) : base(null)
    {
        FileSystem = fileSystem;
    }

    public ProgramFileSystem FileSystem { get; }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Programs with a directory structure can always use the file and delta modules for mods
        builder.Register(new ModModuleComponent(_ => new FilesModule()));
        builder.Register(new ModModuleComponent(_ => new DeltasModule(null)));

        builder.Register(new ModLibraryPathComponent(x => x.InstallLocation.Directory + ".rcp_mods"));
    }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        return FileSystem.IsLocationValid(location);
    }
}