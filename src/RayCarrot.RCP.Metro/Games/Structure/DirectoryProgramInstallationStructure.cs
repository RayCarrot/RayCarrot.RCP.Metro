using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;
using RayCarrot.RCP.Metro.ModLoader.Modules.Files;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class DirectoryProgramInstallationStructure : ProgramInstallationStructure
{
    public DirectoryProgramInstallationStructure(ProgramFileSystem fileSystem)
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
        ProgramFileSystemValidationResult validationResult = FileSystem.IsLocationValid(new PhysicalFileSystemSource(location.Directory), true);

        if (validationResult.IsValid)
        {
            return new GameLocationValidationResult(true);
        }
        else
        {
            return new GameLocationValidationResult(false,
                String.Format(Resources.Games_ValidationMissingPaths,
                    validationResult.InvalidPaths.Select(x => x.Path).Take(10).JoinItems(Environment.NewLine, x => $"- {x}")));
        }
    }
}