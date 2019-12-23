using System.IO;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive file data
    /// </summary>
    public interface IArchiveFileData
    {
        /// <summary>
        /// The directory the file is located under
        /// </summary>
        string Directory { get; }

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

        /// <summary>
        /// The name of the file format
        /// </summary>
        string FileFormatName { get; }

        /// <summary>
        /// The available file formats for the file
        /// </summary>
        FileFilterItemCollection AvailableFileFormats { get; }

        /// <summary>
        /// The path to the temporary file containing the data to be imported
        /// </summary>
        FileSystemPath PendingImportTempPath { get; set; }

        /// <summary>
        /// Saves the file to the specified path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path to save the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        Task SaveFileAsync(Stream archiveFileStream, FileSystemPath filePath, string fileFormat);

        /// <summary>
        /// Imports the file from the specified path to the <see cref="PendingImportTempPath"/> path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path of the file to import</param>
        /// <returns>The task</returns>
        Task ImportFileAsync(Stream archiveFileStream, FileSystemPath filePath);
    }
}