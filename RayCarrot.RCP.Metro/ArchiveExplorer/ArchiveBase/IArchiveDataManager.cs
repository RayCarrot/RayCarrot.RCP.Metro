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
        /// Loads the archive
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        ArchiveData LoadArchive(Stream archiveFileStream);

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The files of the archive. Modified files have the <see cref="IArchiveFileData.PendingImportTempPath"/> property set to an existing path.</param>
        /// <param name="generator">The file generator</param>
        void UpdateArchive(Stream archiveFileStream, Stream outputFileStream, IEnumerable<IArchiveFileData> files, IDisposable generator);
    }
}