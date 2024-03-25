namespace RayCarrot.RCP.Metro.Games.Structure;

public interface IFileSystemSource
{
    FileSystemPath BasePath { get; }

    bool FileExists(string path);
    bool DirectoryExists(string path);
}