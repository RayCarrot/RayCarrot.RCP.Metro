using RayCarrot.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines an archive data manager
    /// </summary>
    public interface IArchiveDataManager
    {
        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        char PathSeparatorCharacter { get; }

        /// <summary>
        /// Loads the archive data
        /// </summary>
        /// <param name="archive">The archive data</param>
        /// <param name="archiveFileStream">The archive file stream</param>
        /// <returns>The archive data</returns>
        ArchiveData LoadArchiveData(object archive, Stream archiveFileStream);

        /// <summary>
        /// Loads the archive
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        object LoadArchive(Stream archiveFileStream);

        /// <summary>
        /// Gets a new archive from a collection of modified files
        /// </summary>
        /// <param name="files"></param>
        /// <returns>The archive</returns>
        object GetArchive(IEnumerable<IArchiveImportData> files);

        /// <summary>
        /// Gets a new file entry from the specified path
        /// </summary>
        /// <param name="relativePath">The relative path of the file</param>
        /// <returns>The file entry</returns>
        object GetFileEntry(FileSystemPath relativePath);

        /// <summary>
        /// Encodes the file bytes
        /// </summary>
        /// <param name="fileData">The bytes to encode</param>
        /// <param name="fileEntry">The file entry for the file to encode</param>
        /// <returns>The encoded bytes</returns>
        byte[] EncodeFile(byte[] fileData, object fileEntry);

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The import data for the archive files</param>
        void UpdateArchive(object archive, Stream outputFileStream, IEnumerable<IArchiveImportData> files);
    }
}