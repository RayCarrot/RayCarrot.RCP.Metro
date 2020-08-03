using RayCarrot.Common;
using RayCarrot.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.Logging;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an archive explorer dialog
    /// </summary>
    public class ArchiveExplorerDialogViewModel : UserInputViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <param name="filePaths">The archive file paths</param>
        public ArchiveExplorerDialogViewModel(IArchiveDataManager manager, IEnumerable<FileSystemPath> filePaths)
        {
            try
            {
                // Set the default title
                Title = Resources.Archive_Title;

                // Get the manager
                Manager = manager;

                // Create the load action
                var load = new Operation(() => IsLoading = true, () => IsLoading = false, true);

                // Get the archives
                Archives = filePaths.Select(x => new ArchiveViewModel(x, manager, load, this)).ToArray();

                // Set the archive lock
                ArchiveLock = new AsyncLock();

                RL.Logger?.LogInformationSource($"The Archive Explorer is loading with {Archives.Length} archives");

                // Make sure we got an archive
                if (!Archives.Any())
                    throw new ArgumentException("At least one archive path needs to be available");

                // Lock when accessing the archive
                using (ArchiveLock.Lock())
                {
                    // Load each archive
                    foreach (var archive in Archives)
                        archive.LoadArchive();
                }

                // Select and expand the first item
                Archives.First().IsSelected = true;
                Archives.First().IsExpanded = true;
            }
            catch
            {
                // Make sure the view model gets disposed
                Dispose();

                throw;
            }
        }

        /// <summary>
        /// Indicates if files are being initialized
        /// </summary>
        public bool IsInitializingFiles { get; set; }

        /// <summary>
        /// Indicates if initializing files should be canceled
        /// </summary>
        public bool CancelInitializeFiles { get; set; }

        /// <summary>
        /// Indicates if a process is running, such as importing/exporting
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// The directories
        /// </summary>
        public ArchiveViewModel[] Archives { get; }

        /// <summary>
        /// The archive data manager
        /// </summary>
        public IArchiveDataManager Manager { get; }

        /// <summary>
        /// The current status to display
        /// </summary>
        public string DisplayStatus { get; set; }

        /// <summary>
        /// The lock to use when accessing any archive stream
        /// </summary>
        public AsyncLock ArchiveLock { get; }

        /// <summary>
        /// Updates the loaded directory thumbnails
        /// </summary>
        /// <returns>The task</returns>
        public async Task ChangeLoadedDirAsync(ArchiveDirectoryViewModel previousDir, ArchiveDirectoryViewModel newDir)
        {
            RL.Logger?.LogDebugSource($"The loaded archive directory is changing from {previousDir?.DisplayName ?? "NULL"} to {newDir?.DisplayName ?? "NULL"}");

            // Stop refreshing thumbnails
            if (IsInitializingFiles)
                CancelInitializeFiles = true;

            RL.Logger?.LogDebugSource($"Updating loaded archive dir from {previousDir?.DisplayName} to {newDir.DisplayName}");

            // Lock the access to the archive
            using (await ArchiveLock.LockAsync())
            {
                try
                {
                    // Check if the operation should be canceled
                    if (CancelInitializeFiles)
                    {
                        RL.Logger?.LogDebugSource($"Canceled initializing files for archive dir {newDir.DisplayName}");

                        return;
                    }

                    // Indicate that we are refreshing the thumbnails
                    IsInitializingFiles = true;

                    RL.Logger?.LogDebugSource($"Initializing files for archive dir {newDir.DisplayName}");

                    // Remove all thumbnail image sources from memory
                    previousDir?.Files.ForEach(x =>
                    {
                        x.IsInitialized = false;
                        x.ThumbnailSource = null;
                    });

                    // Initialize files in the new directory
                    await Task.Run(() =>
                    {
                        // Initialize each file
                        foreach (var x in newDir.Files.
                            // Copy to an array to avoid an exception when the files refresh
                            ToArray())
                        {
                            // Check if the operation should be canceled
                            if (CancelInitializeFiles)
                            {
                                RL.Logger?.LogDebugSource($"Canceled initializing files for archive dir {newDir.DisplayName}");

                                return;
                            }

                            // Initialize the file
                            x.InitializeFile();
                        }
                    });

                    RL.Logger?.LogDebugSource($"Initialized files for archive dir {newDir.DisplayName}");
                }
                finally
                {
                    IsInitializingFiles = false;
                    CancelInitializeFiles = false;
                }
            }
        }

        public void Dispose()
        {
            // Dispose every archive
            Archives?.ForEach(x => x.Dispose());
        }
    }
}