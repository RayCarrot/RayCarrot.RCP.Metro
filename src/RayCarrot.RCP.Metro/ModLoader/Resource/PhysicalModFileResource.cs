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
        // The \\?\ part is a temp fix for dealing with long file paths, an issue that appears with Rayman Legends for many mods
        // This should be removed when we migrate to .NET 8 since that supports long file paths
        using Stream fileStream = File.OpenRead($@"\\?\{FilePath}");
        fileStream.CopyToEx(destinationStream);
    }
}