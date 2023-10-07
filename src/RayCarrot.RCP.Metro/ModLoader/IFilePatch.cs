using System.IO;

namespace RayCarrot.RCP.Metro.ModLoader;

public interface IFilePatch
{
    ModFilePath Path { get; }
    void PatchFile(Stream stream);
}