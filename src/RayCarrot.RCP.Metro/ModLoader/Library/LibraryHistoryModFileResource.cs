using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

/// <summary>
/// A <see cref="IModFileResource"/> which originates from the library file history
/// </summary>
public class LibraryHistoryModFileResource : PhysicalModFileResource
{
    public LibraryHistoryModFileResource(ModFilePath path, FileSystemPath filePath) : base(path, filePath) { }
}