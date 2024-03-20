namespace RayCarrot.RCP.Metro.Games.Structure;

/// <summary>
/// Defines paths for a program. Note that these aren't all the paths and
/// is only to be used as a reference for common and required files.
/// </summary>
public class ProgramFileSystem
{
    public ProgramFileSystem(ProgramPath[] Paths)
    {
        this.Paths = Paths;
    }

    public ProgramPath[] Paths { get; }

    private static IEnumerable<(FileSystemPath FullPath, ProgramPath Path)> EnumeratePaths(FileSystemPath basePath, IEnumerable<ProgramPath> gamePaths)
    {
        foreach (ProgramPath gamePath in gamePaths)
        {
            FileSystemPath fullPath = basePath + gamePath.Path;

            yield return (fullPath, gamePath);

            foreach (var subGamePath in EnumeratePaths(fullPath, gamePath.SubPaths))
                yield return subGamePath;
        }
    }

    public IEnumerable<(FileSystemPath FullPath, ProgramPath Path)> EnumeratePaths(FileSystemPath basePath) =>
        EnumeratePaths(basePath, Paths);

    public IEnumerable<FileSystemPath> GetAbsolutePaths(GameInstallation gameInstallation, ProgramPathType type) =>
        GetAbsolutePaths(gameInstallation.InstallLocation.Directory, type);
    public IEnumerable<FileSystemPath> GetAbsolutePaths(FileSystemPath basePath, ProgramPathType type) =>
        EnumeratePaths(basePath).Where(x => x.Path.Type == type).Select(x => x.FullPath);

    public string GetLocalPath(ProgramPathType type) =>
        GetAbsolutePath(FileSystemPath.EmptyPath, type);
    public FileSystemPath GetAbsolutePath(GameInstallation gameInstallation, ProgramPathType type) =>
        GetAbsolutePath(gameInstallation.InstallLocation.Directory, type);
    public FileSystemPath GetAbsolutePath(FileSystemPath basePath, ProgramPathType type) =>
        EnumeratePaths(basePath).FirstOrDefault(x => x.Path.Type == type).FullPath;
}