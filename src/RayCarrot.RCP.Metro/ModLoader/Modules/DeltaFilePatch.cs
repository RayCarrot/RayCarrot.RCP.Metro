using System.IO;

namespace RayCarrot.RCP.Metro.ModLoader.Modules;

public class DeltaFilePatch : IFilePatch
{
    public DeltaFilePatch(ModFilePath path, FileSystemPath deltaPatchFilePath)
    {
        Path = path;
        DeltaPatchFilePath = deltaPatchFilePath;
    }

    public ModFilePath Path { get; }
    public FileSystemPath DeltaPatchFilePath { get; }
    
    public void PatchFile(Stream stream)
    {
        // TODO-UPDATE: Implement delta patching
    }
}