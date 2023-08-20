using System.IO;

namespace RayCarrot.RCP.Metro.ModLoader.Resource;

/// <summary>
/// A file resource from a mod
/// </summary>
public interface IModFileResource
{
    /// <summary>
    /// The resource path
    /// </summary>
    ModFilePath Path { get; }

    /// <summary>
    /// Reads the file resource as a stream
    /// </summary>
    /// <returns>The file resource stream</returns>
    Stream Read();
}