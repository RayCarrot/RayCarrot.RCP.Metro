using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

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
        using Context context = new RCPContext(DeltaPatchFilePath.Parent);
        DeltaFile deltaFile = context.ReadRequiredFileData<DeltaFile>(DeltaPatchFilePath.Name);

        foreach (DeltaChunk deltaChunk in deltaFile.Chunks)
        {
            stream.Position = deltaChunk.PatchOffset;
            stream.Write(deltaChunk.PatchData, 0, deltaChunk.PatchData.Length);
        }
    }
}