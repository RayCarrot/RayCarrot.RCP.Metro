using System;

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
        /// The function used to get the encoded data to import, passing in the file entry
        /// </summary>
        Func<object, byte[]> GetData { get; }
    }
}