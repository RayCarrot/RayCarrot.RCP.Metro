using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Modules;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class DirectoryProgramInstallationStructure : ProgramInstallationStructure
{
    public DirectoryProgramInstallationStructure(GameInstallationPath[] gamePaths)
    {
        GamePaths = gamePaths;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The paths defined for the game installation. Note that these aren't
    /// all the paths for the game and is only to be used as a reference.
    /// </summary>
    public GameInstallationPath[] GamePaths { get; }

    private static IEnumerable<(FileSystemPath FullPath, GameInstallationPath Path)> EnumeratePaths(FileSystemPath basePath, IEnumerable<GameInstallationPath> gamePaths)
    {
        foreach (GameInstallationPath gamePath in gamePaths)
        {
            FileSystemPath fullPath = basePath + gamePath.Path;

            yield return (fullPath, gamePath);

            foreach (var subGamePath in EnumeratePaths(fullPath, gamePath.SubPaths))
                yield return subGamePath;
        }
    }

    public IEnumerable<(FileSystemPath FullPath, GameInstallationPath Path)> EnumeratePaths(FileSystemPath basePath) =>
        EnumeratePaths(basePath, GamePaths);

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Programs with a directory structure can always use the file and delta modules for mods
        builder.Register(new ModModuleComponent(_ => new FilesModule()));
        builder.Register(new ModModuleComponent(_ => new DeltasModule()));
    }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        List<string>? invalidPaths = null;

        foreach ((FileSystemPath FullPath, GameInstallationPath Path) in EnumeratePaths(location.Directory))
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

    public IEnumerable<FileSystemPath> GetAbsolutePaths(GameInstallation gameInstallation, GameInstallationPathType type) =>
        GetAbsolutePaths(gameInstallation.InstallLocation.Directory, type);
    public IEnumerable<FileSystemPath> GetAbsolutePaths(FileSystemPath basePath, GameInstallationPathType type) =>
        EnumeratePaths(basePath).Where(x => x.Path.Type == type).Select(x => x.FullPath);

    public string GetLocalPath(GameInstallationPathType type) =>
        GetAbsolutePath(FileSystemPath.EmptyPath, type);
    public FileSystemPath GetAbsolutePath(GameInstallation gameInstallation, GameInstallationPathType type) =>
        GetAbsolutePath(gameInstallation.InstallLocation.Directory, type);
    public FileSystemPath GetAbsolutePath(FileSystemPath basePath, GameInstallationPathType type) =>
        EnumeratePaths(basePath).FirstOrDefault(x => x.Path.Type == type).FullPath;

}