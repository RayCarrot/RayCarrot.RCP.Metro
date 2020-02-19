using System;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Import data for an original, not modified, file
    /// </summary>
    public class OriginalArchiveImportData : IArchiveImportData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileEntryData">The file entry data</param>
        public OriginalArchiveImportData(object fileEntryData)
        {
            FileEntryData = fileEntryData;
        }

        /// <summary>
        /// The file entry data
        /// </summary>
        public object FileEntryData { get; }

        /// <summary>
        /// Indicates if the file has been modified
        /// </summary>
        public bool IsModified => false;

        /// <summary>
        /// The stream to the file contents, if the file has been modified
        /// </summary>
        public Stream GetDataStream => throw new InvalidOperationException("The import data is not modified");
    }
}