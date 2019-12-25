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
        /// Loads the archive
        /// </summary>
        public void LoadArchive()
        {
            // Clear existing items
            Files.Clear();
            Clear();

            // Get the archive directories
            var dirs = Manager.GetDirectories(ArchiveFileStream);

            // Add each directory
            foreach (var dir in dirs)
            {
                // Check if it's the root directory
                if (dir.DirectoryName == String.Empty)
                {
                    // Add the files
                    Files.AddRange(dir.Files.Select(x => new ArchiveFileViewModel(x, this)));

                    continue;
                }

                // Keep track of the previous item
                ArchiveDirectoryViewModel prevItem = this;

                // Enumerate each sub directory
                foreach (string subDir in dir.DirectoryName.Split('\\'))
                {
                    // Set the previous item and create the item if it doesn't already exist
                    prevItem = prevItem.FindItem(x => x.ID == subDir) ?? prevItem.Add(subDir);
                }

                // Add the files
                prevItem.Files.AddRange(dir.Files.Select(x => new ArchiveFileViewModel(x, this)));
            }
        }

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
            
            // Reload the archive
            LoadArchive();

            // Find the selected item
            var selected = this.GetAllChildren<ArchiveDirectoryViewModel>(true).FindItem(x => x.IsSelected);

            // Refresh the selection to refresh the thumbnails
            selected.IsSelected = false;
            selected.IsSelected = true;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            ArchiveFileStream?.Dispose();
        }
    }
}