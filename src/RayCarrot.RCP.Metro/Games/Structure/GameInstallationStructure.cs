namespace RayCarrot.RCP.Metro.Games.Structure;

// TODO: In the future we can expand this to allow for files on a disc (iso/bin/cue), in a package (apk etc.) and offsets (roms)
public class GameInstallationStructure
{
    public GameInstallationStructure(GameInstallationPath[] gamePaths)
    {
        GamePaths = gamePaths;
    }

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

    public bool IsLocationValid(FileSystemPath location)
    {
        foreach ((FileSystemPath FullPath, GameInstallationPath Path) in EnumeratePaths(location))
        {
            if (!Path.Required)
                continue;

            if (!Path.IsValid(FullPath))
                return false;
        }

        return true;
    }

    public IEnumerable<FileSystemPath> GetAbsolutePaths(GameInstallation gameInstallation, GameInstallationPathType type) =>
        GetAbsolutePaths(gameInstallation.InstallLocation, type);
    public IEnumerable<FileSystemPath> GetAbsolutePaths(FileSystemPath basePath, GameInstallationPathType type) =>
        EnumeratePaths(basePath).Where(x => x.Path.Type == type).Select(x => x.FullPath);

    public string GetLocalPath(GameInstallationPathType type) =>
        GetAbsolutePath(FileSystemPath.EmptyPath, type);
    public FileSystemPath GetAbsolutePath(GameInstallation gameInstallation, GameInstallationPathType type) =>
        GetAbsolutePath(gameInstallation.InstallLocation, type);
    public FileSystemPath GetAbsolutePath(FileSystemPath basePath, GameInstallationPathType type) =>
        EnumeratePaths(basePath).FirstOrDefault(x => x.Path.Type == type).FullPath;
}