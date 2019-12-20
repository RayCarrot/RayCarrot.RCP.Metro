using System;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an archive from a file
    /// </summary>
    public class ArchiveViewModel : ArchiveDirectoryViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filePath">The file path for the archive</param>
        public ArchiveViewModel(FileSystemPath filePath) : base(filePath.Name)
        {
            // Get the file path
            FilePath = filePath;

            // Create the file stream
            ArchiveFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// The archive file stream
        /// </summary>
        public FileStream ArchiveFileStream { get; }

        /// <summary>
        /// The file path for the archive
        /// </summary>
        public FileSystemPath FilePath { get; }

        /// <summary>
        /// The name of the item to display
        /// </summary>
        public override string DisplayName => FilePath.Name;

        public void Dispose()
        {
            ArchiveFileStream?.Dispose();
        }
    }
}