using System;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Import data for a modified file
    /// </summary>
    public class ModifiedArchiveImportData : IArchiveImportData, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileEntryData">The file entry data</param>
        /// <param name="tempFile">The temporary file for the data to be imported</param>
        public ModifiedArchiveImportData(object fileEntryData, TempFile tempFile)
        {
            FileEntryData = fileEntryData;
            TempFile = tempFile;
        }

        /// <summary>
        /// The temporary file for the data to be imported
        /// </summary>
        protected TempFile TempFile { get; }

        /// <summary>
        /// The file entry data
        /// </summary>
        public object FileEntryData { get; }

        /// <summary>
        /// Indicates if the file has been modified
        /// </summary>
        public bool IsModified => true;

        /// <summary>
        /// The stream to the file contents, if the file has been modified
        /// </summary>
        public Stream GetDataStream => File.Open(TempFile.TempPath, FileMode.Open, FileAccess.ReadWrite);

        /// <summary>
        /// Disposes the temporary file data
        /// </summary>
        public void Dispose()
        {
            TempFile?.Dispose();
        }
    }
}