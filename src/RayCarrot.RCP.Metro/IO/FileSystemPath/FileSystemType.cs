namespace RayCarrot.RCP.Metro;

/// <summary>
/// A type of file system entry
/// </summary>
public enum FileSystemType
{
    /// <summary>
    /// A file
    /// </summary>
    File,

    /// <summary>
    /// A directory
    /// </summary>
    Directory,

    /// <summary>
    /// A relative path
    /// </summary>
    Relative,

    /// <summary>
    /// An unknown entry
    /// </summary>
    Unknown
}