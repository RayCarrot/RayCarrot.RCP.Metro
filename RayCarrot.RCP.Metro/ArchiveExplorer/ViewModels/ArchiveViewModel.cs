using Nito.AsyncEx;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
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
        /// <param name="explorerDialogViewModel">The explorer dialog view model</param>
        public ArchiveViewModel(FileSystemPath filePath, IArchiveDataManager manager, Operation loadOperation, ArchiveExplorerDialogViewModel explorerDialogViewModel) : base(filePath.Name)
        {
            // Set properties
            FilePath = filePath;
            Manager = manager;
            LoadOperation = loadOperation;
            ExplorerDialogViewModel = explorerDialogViewModel;
            
            // Create the file stream
            ArchiveFileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The explorer dialog view model
        /// </summary>
        protected ArchiveExplorerDialogViewModel ExplorerDialogViewModel { get; }

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
        public FileStream ArchiveFileStream { get; protected set; }

        /// <summary>
        /// The lock to use when accessing the archive stream
        /// </summary>
        public AsyncLock ArchiveLock => ExplorerDialogViewModel.ArchiveLock;

        /// <summary>
        /// The file path for the archive
        /// </summary>
        public FileSystemPath FilePath { get; }

        /// <summary>
        /// The description to display
        /// </summary>
        public override string DisplayDescription => FilePath.FullPath;

        /// <summary>
        /// The name of the item to display
        /// </summary>
        public override string DisplayName => FilePath.Name;

        /// <summary>
        /// The archive data manager
        /// </summary>
        public IArchiveDataManager Manager { get; }

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
            // Dispose every directory
            this.GetAllChildren<ArchiveDirectoryViewModel>().DisposeAll();

            // Dispose the files
            Files.DisposeAll();

            // Clear the items
            Clear();
            Files.Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the display status
        /// </summary>
        /// <param name="status">The status to display</param>
        public void SetDisplayStatus(string status)
        {
            ExplorerDialogViewModel.DisplayStatus = status;
        }

        /// <summary>
        /// Loads the archive
        /// </summary>
        public void LoadArchive()
        {
            // Clear existing items
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

                // IDEA: Have this configurable in manager?
                var separatorChars = new char[]
                {
                    '\\', 
                    '/'
                };

                // Enumerate each sub directory
                foreach (string subDir in dir.DirectoryName.Trim(separatorChars).Split(separatorChars))
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
        /// <returns>A value indicating if the updating succeeded</returns>
        public async Task<bool> UpdateArchiveAsync()
        {
            // Stop refreshing thumbnails
            if (ExplorerDialogViewModel.IsRefreshingThumbnails)
                ExplorerDialogViewModel.CancelRefreshingThumbnails = true;

            Archive.SetDisplayStatus(String.Format(Resources.Archive_RepackingStatus, DisplayName));

            // Find the selected item ID
            var selected = SelectedItem.FullID;

            // Flag for if the update succeeded
            var succeeded = false;

            try
            {
                // Get a temporary file path to write to
                using var tempOutputFile = new TempFile(false);

                // Create the file and get the stream
                using (var outputStream = File.Create(tempOutputFile.TempPath))
                {
                    // Get the files
                    var files = this.GetAllChildren<ArchiveDirectoryViewModel>(true).SelectMany(x => x.Files).Select(x => x.FileData);

                    // Update the archive
                    Manager.UpdateArchive(ArchiveFileStream, outputStream, files);
                }

                // Dispose the archive file stream
                ArchiveFileStream.Dispose();

                ArchiveFileStream = null;

                // If the operation succeeded, replace the archive file with the temporary output
                RCFRCP.File.MoveFile(tempOutputFile.TempPath, FilePath, true);

                // Re-open the file stream
                ArchiveFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

                succeeded = true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Repacking archive", DisplayName);

                // TODO: Localize
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred repacking the archive");

                // Re-open the file stream if closed
                if (ArchiveFileStream == null)
                    ArchiveFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            }

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

            // If the item is selected, simply reload the thumbnails, but without awaiting it
            if (previouslySelectedItem.IsSelected)
                // Run async without awaiting
                _ = ExplorerDialogViewModel.ChangeLoadedDirAsync(null, previouslySelectedItem);
            // Otherwise select the item and let the thumbnails get automatically reloaded
            else
                previouslySelectedItem.IsSelected = true;

            Archive.SetDisplayStatus(String.Empty);

            return succeeded;
        }

        public override void Dispose()
        {
            // Cancel refreshing thumbnails
            ExplorerDialogViewModel.CancelRefreshingThumbnails = true;

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