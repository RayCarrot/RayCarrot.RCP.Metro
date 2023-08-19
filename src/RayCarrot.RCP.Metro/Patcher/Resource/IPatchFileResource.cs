using System.IO;

namespace RayCarrot.RCP.Metro.Patcher.Resource;

/// <summary>
/// A file resource from a patch
/// </summary>
public interface IPatchFileResource
{
    /// <summary>
    /// The resource path
    /// </summary>
    PatchFilePath Path { get; }

    /// <summary>
    /// Reads the file resource as a stream
    /// </summary>
    /// <returns>The file resource stream</returns>
    Stream Read();
}