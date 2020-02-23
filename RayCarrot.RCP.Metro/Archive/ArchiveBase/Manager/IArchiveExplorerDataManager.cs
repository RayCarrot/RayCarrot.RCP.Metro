using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines an archive explorer data manager
    /// </summary>
    public interface IArchiveExplorerDataManager : IArchiveBaseDataManager
    {
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
    }
}