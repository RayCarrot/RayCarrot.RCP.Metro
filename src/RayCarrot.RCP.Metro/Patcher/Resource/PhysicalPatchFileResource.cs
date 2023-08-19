using System.IO;

namespace RayCarrot.RCP.Metro.Patcher.Resource;

public class PhysicalPatchFileResource : IPatchFileResource
{
    public PhysicalPatchFileResource(PatchFilePath path, FileSystemPath filePath)
    {
        Path = path;
        FilePath = filePath;
    }

    public PatchFilePath Path { get; }
    public FileSystemPath FilePath { get; }

    public Stream Read() => File.OpenRead(FilePath);
}