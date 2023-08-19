using System.IO;

namespace RayCarrot.RCP.Metro.Patcher.Resource;

public class VirtualPatchFileResource : IPatchFileResource
{
    public VirtualPatchFileResource(PatchFilePath path, Stream stream)
    {
        Path = path;
        Stream = stream;
    }

    public PatchFilePath Path { get; }
    public Stream Stream { get; }

    public Stream Read() => Stream;
}