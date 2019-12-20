using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive file data
    /// </summary>
    public interface IArchiveFileData
    {
        /// <summary>
        /// The file name
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The contents of the file</returns>
        byte[] GetFileContent(Stream archiveFileStream);
    }
}