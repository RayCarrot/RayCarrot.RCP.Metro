using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Import data for an archive file
    /// </summary>
    public class ArchiveImportData : IArchiveImportData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileEntryData">The file entry data</param>
        /// <param name="getData">The function used to get the encoded data to import, passing in the file entry</param>
        public ArchiveImportData(object fileEntryData, Func<object, byte[]> getData)
        {
            FileEntryData = fileEntryData;
            GetData = getData;
        }

        /// <summary>
        /// The file entry data
        /// </summary>
        public object FileEntryData { get; }

        /// <summary>
        /// The function used to get the encoded data to import, passing in the file entry
        /// </summary>
        public Func<object, byte[]> GetData { get; }
    }
}