using MahApps.Metro.IconPacks;
using RayCarrot.IO;
using System;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines archive file data
    /// </summary>
    public interface IArchiveFileData
    {
        /// <summary>
        /// The file entry data
        /// </summary>
        object FileEntryData { get; }

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
        /// Gets the decoded contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="generator">The file generator</param>
        /// <param name="initilizeOnly">Indicates if the bytes should be retrieved for initialization only, in which case the bytes don't need to be returned</param>
        /// <returns>The contents of the file</returns>
        byte[] GetDecodedFileBytes(Stream archiveFileStream, IDisposable generator, bool initilizeOnly);

        /// <summary>
        /// Gets the original encoded contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="generator">The file generator</param>
        /// <returns>The contents of the file</returns>
        byte[] GetEncodedFileBytes(Stream archiveFileStream, IDisposable generator);

        /// <summary>
        /// The default icon to use for this file
        /// </summary>
        PackIconMaterialKind IconKind { get; }

        /// <summary>
        /// The name of the file format
        /// </summary>
        string FileFormatName { get; }

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        FileExtension[] SupportedImportFileExtensions { get; }

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        FileExtension[] SupportedExportFileExtensions { get; }

        /// <summary>
        /// Exports the file to the stream in the specified format
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="outputStream">The stream to export to</param>
        /// <param name="format">The file format to use</param>
        void ExportFile(byte[] fileBytes, Stream outputStream, FileExtension format);

        /// <summary>
        /// Converts the import file data from the input stream to the output stream
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="inputStream">The input stream to import from</param>
        /// <param name="outputStream">The destination stream</param>
        /// <param name="format">The file format to use</param>
        void ConvertImportData(byte[] fileBytes, Stream inputStream, Stream outputStream, FileExtension format);
    }
}