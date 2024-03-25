namespace RayCarrot.RCP.Metro.Games.Structure;

public class PhysicalFileSystemSource : IFileSystemSource
{
    public PhysicalFileSystemSource(FileSystemPath basePath)
    {
        BasePath = basePath;
    }

    public FileSystemPath BasePath { get; }

    public bool FileExists(string path) => (BasePath + path).FileExists;
    public bool DirectoryExists(string path) => (BasePath + path).DirectoryExists;
}