using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="manager">The archive data manager</param>
        public ArchiveViewModel(FileSystemPath filePath, IArchiveDataManager manager) : base(filePath.Name)
        {
            // Set properties
            FilePath = filePath;
            Manager = manager;

            // Create the file stream
            ArchiveFileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
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

        /// <summary>
        /// The archive data manager
        /// </summary>
        public IArchiveDataManager Manager { get; }

        /// <summary>
        /// Updates the archive with the pending imports
        /// </summary>
        public Task UpdateArchiveAsync()
        {
            // TODO: Progress bar & error handling

            // Get the modified files
            var files = this.GetAllChildren<ArchiveDirectoryViewModel>(true).SelectMany(x => x.Files).Select(x => x.FileData).Where(x => x.PendingImportTempPath.FileExists);

            // Update the archive
            Manager.UpdateArchive(ArchiveFileStream, files);
            
            // Update thumbnails for selected directory
            this.GetAllChildren<ArchiveDirectoryViewModel>().FindItem(x => x.IsSelected)?.Files.ForEach(y => y.LoadThumbnail());

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            ArchiveFileStream?.Dispose();
        }
    }
}