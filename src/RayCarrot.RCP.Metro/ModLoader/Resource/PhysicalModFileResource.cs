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

    public void CopyToStream(Stream destinationStream)
    {
        using Stream fileStream = File.OpenRead(FilePath.ToLongPath());
        fileStream.CopyToEx(destinationStream);
    }
}