using System;
using System.IO;

namespace RayCarrot.RCP.Metro.Archive
{
    /// <summary>
    /// Defines an archive file generator used to get the file bytes from a file entry
    /// </summary>
    /// <typeparam name="FileEntry">The type of file entry</typeparam>
    public interface IArchiveFileGenerator<in FileEntry> : IDisposable
    {
        /// <summary>
        /// Gets the number of files which can be retrieved from the generator
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the file stream for the specified key
        /// </summary>
        /// <param name="fileEntry">The file entry to get the stream for</param>
        /// <returns>The stream</returns>
        Stream GetFileStream(FileEntry fileEntry);
    }
}