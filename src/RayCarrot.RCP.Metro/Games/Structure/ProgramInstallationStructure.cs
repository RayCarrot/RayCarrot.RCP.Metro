namespace RayCarrot.RCP.Metro.Games.Structure;

// TODO: In the future we can expand this to allow for files on a disc (iso/bin/cue), in a package (apk etc.) and offsets (roms)
public class ProgramInstallationStructure
{
    public ProgramInstallationStructure(GameInstallationPath[] gamePaths)
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

    public GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        // TODO-14: Validate the location if it's a file instead
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

            // TODO-UPDATE: Localize
            return new GameLocationValidationResult(false, $"The following required paths were not found:{Environment.NewLine}{invalidPaths.Take(10).JoinItems(Environment.NewLine, x => $"- {x}")}");
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