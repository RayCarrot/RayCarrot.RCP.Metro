using System.IO;

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

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ProgramPath[] Paths { get; }

    private static IEnumerable<(string FullPath, ProgramPath Path)> EnumeratePaths(string basePath, IEnumerable<ProgramPath> gamePaths)
    {
        foreach (ProgramPath gamePath in gamePaths)
        {
            FileSystemPath fullPath = Path.Combine(basePath, gamePath.Path);

            yield return (fullPath, gamePath);

            foreach (var subGamePath in EnumeratePaths(fullPath, gamePath.SubPaths))
                yield return subGamePath;
        }
    }

    public IEnumerable<(string FullPath, ProgramPath Path)> EnumeratePaths() => 
        EnumeratePaths(String.Empty, Paths);

    public IEnumerable<FileSystemPath> GetAbsolutePaths(GameInstallation gameInstallation, ProgramPathType type) =>
        GetAbsolutePaths(gameInstallation.InstallLocation.Directory, type);
    public IEnumerable<FileSystemPath> GetAbsolutePaths(FileSystemPath basePath, ProgramPathType type) =>
        EnumeratePaths().Where(x => x.Path.Type == type).Select(x => basePath + x.FullPath);

    public string GetLocalPath(ProgramPathType type) =>
        GetAbsolutePath(FileSystemPath.EmptyPath, type);
    public FileSystemPath GetAbsolutePath(GameInstallation gameInstallation, ProgramPathType type) =>
        GetAbsolutePath(gameInstallation.InstallLocation.Directory, type);
    public FileSystemPath GetAbsolutePath(FileSystemPath basePath, ProgramPathType type)
    {
        string? fullPath = EnumeratePaths().FirstOrDefault(x => x.Path.Type == type).FullPath;

        if (fullPath == null)
            return FileSystemPath.EmptyPath;

        return basePath + fullPath;
    }

    public ProgramFileSystemValidationResult IsLocationValid(IFileSystemSource source, bool checkAllPaths)
    {
        List<ProgramPath>? invalidPaths = null;

        foreach ((string FullPath, ProgramPath Path) in EnumeratePaths())
        {
            if (!Path.Required)
                continue;

            if (!Path.IsValid(source, FullPath))
            {
                invalidPaths ??= new List<ProgramPath>();
                invalidPaths.Add(Path);

                if (!checkAllPaths)
                    break;
            }
        }

        if (invalidPaths == null)
        {
            return new ProgramFileSystemValidationResult(true);
        }
        else
        {
            Logger.Info("The validation for the location {0} failed due to {1} paths not being valid", source.BasePath, invalidPaths.Count);
            return new ProgramFileSystemValidationResult(false, invalidPaths);
        }
    }
}