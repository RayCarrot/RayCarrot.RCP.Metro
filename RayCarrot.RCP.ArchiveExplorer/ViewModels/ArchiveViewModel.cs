using Nito.AsyncEx;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.ArchiveExplorer
{
    /// <summary>
    /// View model for an archive from a file
    /// </summary>
    public class ArchiveViewModel : ArchiveDirectoryViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filePath">The file path for the archive</param>
        /// <param name="manager">The archive data manager</param>
        /// <param name="loadOperation">The operation to use when running an async operation which needs to load</param>
        public ArchiveViewModel(FileSystemPath filePath, IArchiveDataManager manager, Operation loadOperation) : base(filePath.Name)
        {
            // Set properties
            FilePath = filePath;
            Manager = manager;
            LoadOperation = loadOperation;
            AsyncLock = new AsyncLock();

            // Create the file stream
            ArchiveFileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Indicates if thumbnails are being refreshed
        /// </summary>
        private bool IsRefreshingThumbnails { get; set; }

        /// <summary>
        /// Indicates if refreshing the thumbnails should be canceled
        /// </summary>
        private bool CancelRefreshingThumbnails { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The operation to use when running an async operation which needs to load
        /// </summary>
        public Operation LoadOperation { get; }

        /// <summary>
        /// The archive the directory belongs to
        /// </summary>
        public override ArchiveViewModel Archive => this;

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
        /// The async lock to use for refreshing the thumbnails
        /// </summary>
        public AsyncLock AsyncLock { get; }

        /// <summary>
        /// Gets the currently selected item
        /// </summary>
        public ArchiveDirectoryViewModel SelectedItem => this.GetAllChildren<ArchiveDirectoryViewModel>(true).FindItem(x => x.IsSelected);

        #endregion

        #region Protected Methods

        /// <summary>
        /// Clears and disposes every item
        /// </summary>
        protected void ClearAndDisposeItems()
        {
            // Dispose every item
            foreach (var item in this.GetAllChildren<ArchiveDirectoryViewModel>())
                // Dispose the item
                item?.Dispose();

            // Clear the items
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the loaded directory thumbnails
        /// </summary>
        /// <returns>The task</returns>
        public async Task ChangeLoadedDirAsync(ArchiveDirectoryViewModel previousDir, ArchiveDirectoryViewModel newDir)
        {
            if (IsRefreshingThumbnails)
                CancelRefreshingThumbnails = true;

            // Lock to avoid it running multiple times at once
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    // Indicate that we are refreshing the thumbnails
                    IsRefreshingThumbnails = true;

                    // Remove all thumbnail image sources from memory
                    previousDir?.Files.ForEach(x => x.ThumbnailSource = null);

                    // Load the thumbnail image sources for the new directory
                    await Task.Run(() =>
                    {
                        // Load the thumbnail for each file
                        foreach (var x in newDir.Files)
                        {
                            // Load the thumbnail
                            x.LoadThumbnail();

                            // Check if the operation should be canceled
                            if (CancelRefreshingThumbnails)
                                return;
                        }
                    });
                }
                finally
                {
                    IsRefreshingThumbnails = false;
                    CancelRefreshingThumbnails = false;
                }
            }
        }

        /// <summary>
        /// Loads the archive
        /// </summary>
        public void LoadArchive()
        {
            // Clear existing items
            Files.Clear();
            ClearAndDisposeItems();

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
            // TODO: Localize
            DisplayStatus = $"Repacking archive {DisplayName}";

            // TODO: Error handling

            // Find the selected item ID
            var selected = SelectedItem.FullID;

            // Get the modified files
            var files = this.GetAllChildren<ArchiveDirectoryViewModel>(true).SelectMany(x => x.Files).Select(x => x.FileData).Where(x => x.PendingImportTempPath.FileExists);

            // Update the archive
            Manager.UpdateArchive(ArchiveFileStream, files);

            // Reload the archive
            LoadArchive();

            // Get the previously selected item
            var previouslySelectedItem = this.GetAllChildren<ArchiveDirectoryViewModel>(true).FindItem(x => x.FullID.SequenceEqual(selected));

            // Expand the parent items
            var parent = previouslySelectedItem;

            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent;
            }

            // Deselect if already selected
            if (previouslySelectedItem.IsSelected)
                previouslySelectedItem.IsSelected = false;

            // Select the previously selected item
            previouslySelectedItem.IsSelected = true;

            DisplayStatus = String.Empty;

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            // Dispose base class
            base.Dispose();

            // Dispose the stream
            ArchiveFileStream?.Dispose();

            // Dispose every directory
            ClearAndDisposeItems();
        }

        #endregion
    }
}