using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines an archive creator data manager
    /// </summary>
    public interface IArchiveCreatorDataManager : IArchiveBaseDataManager
    {
        /// <summary>
        /// The default archive file name to use when creating an archive
        /// </summary>
        string DefaultArchiveFileName { get; }

        /// <summary>
        /// Gets the configuration UI to use for creator
        /// </summary>
        object GetCreatorUIConfig { get; }

        /// <summary>
        /// Gets a new archive from a collection of modified files
        /// </summary>
        /// <param name="files">The files to import</param>
        /// <returns>The archive creation data</returns>
        ArchiveCreationData GetArchive(IEnumerable<FileSystemPath> files);
    }
}