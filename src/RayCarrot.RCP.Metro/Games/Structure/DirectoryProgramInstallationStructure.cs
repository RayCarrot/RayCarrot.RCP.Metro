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

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        List<string>? invalidPaths = null;

        foreach ((FileSystemPath FullPath, ProgramPath Path) in FileSystem.EnumeratePaths(location.Directory))
        {
            if (!Path.Required)
                continue;

            if (!Path.IsValid(FullPath))
            {
                invalidPaths ??= new List<string>();
                invalidPaths.Add(Path.Path);
            }
        }

        if (invalidPaths == null)
        {
            return new GameLocationValidationResult(true);
        }
        else
        {
            Logger.Info("The validation for the location {0} failed due to {1} paths not being valid", location, invalidPaths.Count);

            return new GameLocationValidationResult(false,
                String.Format(Resources.Games_ValidationMissingPaths,
                    invalidPaths.Take(10).JoinItems(Environment.NewLine, x => $"- {x}")));
        }
    }
}