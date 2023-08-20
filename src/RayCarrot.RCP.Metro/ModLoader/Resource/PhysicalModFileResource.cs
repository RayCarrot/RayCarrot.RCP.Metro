using System.IO;

namespace RayCarrot.RCP.Metro.ModLoader.Resource;

public class PhysicalModFileResource : IModFileResource
{
    public PhysicalModFileResource(ModFilePath path, FileSystemPath filePath)
    {
        Path = path;
        FilePath = filePath;
    }

    public ModFilePath Path { get; }
    public FileSystemPath FilePath { get; }

    public Stream Read() => File.OpenRead(FilePath);
}