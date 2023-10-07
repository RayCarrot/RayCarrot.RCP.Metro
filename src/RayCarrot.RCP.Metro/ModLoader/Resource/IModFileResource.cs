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
    /// Copies the file resource to the destination stream
    /// </summary>
    /// <param name="destinationStream">The stream to copy the resource to</param>
    void CopyToStream(Stream destinationStream);
}