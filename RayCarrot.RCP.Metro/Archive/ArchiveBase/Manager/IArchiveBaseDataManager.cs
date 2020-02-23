using System.Collections.Generic;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines a base archive data manager
    /// </summary>
    public interface IArchiveBaseDataManager
    {
        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        char PathSeparatorCharacter { get; }

        /// <summary>
        /// The file filter to use for the archive files
        /// </summary>
        string ArchiveFileFilter { get; }

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