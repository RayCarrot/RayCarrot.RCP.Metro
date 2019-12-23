using System.Collections.Generic;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// An archive data manager
    /// </summary>
    public interface IArchiveDataManager
    {
        /// <summary>
        /// Gets the available directories from the archive along with their contents
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The directories</returns>
        IEnumerable<ArchiveDirectory> GetDirectories(Stream archiveFileStream);

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="modifiedFiles">The modified files to update in the archive</param>
        void UpdateArchive(Stream archiveFileStream, IEnumerable<IArchiveFileData> modifiedFiles);
    }
}