using Nito.AsyncEx;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
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
            RCFCore.Logger?.LogInformationSource($"An archive view model is being created for {filePath.Name}");

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
        /// The archive file generator
        /// </summary>
        public IDisposable ArchiveFileGenerator { get; set; }

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
        /// The data for the loaded archive
        /// </summary>
        public object ArchiveData { get; set; }

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
            RCFCore.Logger?.LogInformationSource($"The archive items have been cleared and disposed");

            // Dispose every directory
            this.GetAllChildren<ArchiveDirectoryViewModel>().DisposeAll();

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
            RCFCore.Logger?.LogInformationSource($"The archive {DisplayName} is being loaded");

            // Clear existing items
            ClearAndDisposeItems();

            // Load the archive data
            ArchiveData = Manager.LoadArchive(ArchiveFileStream);

            // Load the archive
            var data = Manager.LoadArchiveData(ArchiveData, ArchiveFileStream);

            // Dispose the current generator
            ArchiveFileGenerator?.Dispose();

            // Save the generator
            ArchiveFileGenerator = data.Generator;

            // Add each directory
            foreach (var dir in data.Directories)
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
                foreach (string subDir in dir.DirectoryName.Trim(Manager.PathSeparatorCharacter).Split(Manager.PathSeparatorCharacter))
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
        /// <param name="modifiedImportData">The import data for the files which have been modified</param>
        /// <returns>A value indicating if the updating succeeded</returns>
        public async Task<bool> UpdateArchiveAsync(IEnumerable<ArchiveImportData> modifiedImportData)
        {
            RCFCore.Logger?.LogInformationSource($"The archive {DisplayName} is being updated");

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
                    // Get the modified files
                    var modified = modifiedImportData.ToArray();

                    // Get the non-modified files
                    var origFiles = this.
                        // Get all directories
                        GetAllChildren<ArchiveDirectoryViewModel>(true).
                        // Get the files
                        SelectMany(x => x.Files).
                        // Get the file data
                        Select(x => x.FileData).
                        // Make sure the file isn't one of the modified ones
                        Where(x => modified.All(y => y.FileEntryData != x.FileEntryData)).
                        // Get the import data
                        Select(x => (IArchiveImportData)new ArchiveImportData(x.FileEntryData, file => x.GetEncodedFileBytes(ArchiveFileStream, ArchiveFileGenerator)));

                    // Update the archive
                    Manager.UpdateArchive(ArchiveData, outputStream, modified.Concat(origFiles), ArchiveFileGenerator);
                }

                // Dispose the archive file stream
                ArchiveFileStream.Dispose();

                ArchiveData = null;
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

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_RepackError);

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

            // Dispose the generator
            ArchiveFileGenerator?.Dispose();

            RCFCore.Logger?.LogInformationSource($"The archive {DisplayName} has been disposed");
        }

        #endregion
    }
}