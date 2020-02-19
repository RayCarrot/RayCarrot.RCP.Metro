using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines data for a file which is to be imported to an archive
    /// </summary>
    public interface IArchiveImportData
    {
        /// <summary>
        /// The file entry data
        /// </summary>
        object FileEntryData { get; }

        /// <summary>
        /// Indicates if the file has been modified
        /// </summary>
        bool IsModified { get; }

        /// <summary>
        /// The stream to the file contents, if the file has been modified
        /// </summary>
        Stream GetDataStream { get; }
    }
}