using RayCarrot.RCP.Metro.Patcher.Resource;

namespace RayCarrot.RCP.Metro.Patcher.Library;

/// <summary>
/// A <see cref="IPatchFileResource"/> which originates from the library file history
/// </summary>
public class LibraryHistoryPatchFileResource : PhysicalPatchFileResource
{
    public LibraryHistoryPatchFileResource(PatchFilePath path, FileSystemPath filePath) : base(path, filePath) { }
}