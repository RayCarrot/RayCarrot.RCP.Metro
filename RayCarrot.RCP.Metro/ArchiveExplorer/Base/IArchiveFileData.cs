using System.IO;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines archive file data
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
        /// The info about the file to display
        /// </summary>
        string FileDisplayInfo { get; }

        /// <summary>
        /// Gets the contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The contents of the file</returns>
        byte[] GetFileBytes(Stream archiveFileStream);

        /// <summary>
        /// The name of the file format
        /// </summary>
        string FileFormatName { get; }

        /// <summary>
        /// The supported file formats to import/export from
        /// </summary>
        string[] SupportedFileExtensions { get; }

        /// <summary>
        /// The path to the temporary file containing the data to be imported
        /// </summary>
        FileSystemPath PendingImportTempPath { get; set; }

        /// <summary>
        /// Exports the file to the specified path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        Task ExportFileAsync(Stream archiveFileStream, FileSystemPath filePath, string fileFormat);

        /// <summary>
        /// Imports the file from the specified path to the <see cref="PendingImportTempPath"/> path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path of the file to import</param>
        /// <returns>A value indicating if the file was successfully imported</returns>
        Task<bool> ImportFileAsync(Stream archiveFileStream, FileSystemPath filePath);
    }
}