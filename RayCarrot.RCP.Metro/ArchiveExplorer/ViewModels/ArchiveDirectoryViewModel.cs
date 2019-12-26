using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a directory in an archive
    /// </summary>
    public class ArchiveDirectoryViewModel : HierarchicalViewModel<ArchiveDirectoryViewModel>, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Creates a directory item view model with a directory name from an instance
        /// </summary>
        /// <param name="dirName">The directory name</param>
        protected ArchiveDirectoryViewModel(string dirName) : base(dirName)
        {
            // Create the file collection
            Files = new ObservableCollection<ArchiveFileViewModel>();

            // Create commands
            ExportCommand = new AsyncRelayCommand(ExportAsync);
            ImportCommand = new AsyncRelayCommand(ImportAsync);

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(Files, Application.Current);
            BindingOperations.EnableCollectionSynchronization(this, Application.Current);
        }

        /// <summary>
        /// Creates a directory item view model with the parent and directory name
        /// </summary>
        /// <param name="parent">The parent directory</param>
        /// <param name="dirName">The directory name</param>
        /// <param name="archive">The archive the directory belongs to</param>
        protected ArchiveDirectoryViewModel(ArchiveDirectoryViewModel parent, string dirName, ArchiveViewModel archive) : base(parent, dirName)
        {
            // Set properties
            Archive = archive;

            // Create the file collection
            Files = new ObservableCollection<ArchiveFileViewModel>();

            // Create commands
            ExportCommand = new AsyncRelayCommand(ExportAsync);
            ImportCommand = new AsyncRelayCommand(ImportAsync);
        }

        #endregion

        #region Private Fields

        private bool _isSelected;

        #endregion

        #region Public Properties

        /// <summary>
        /// The archive the directory belongs to
        /// </summary>
        public virtual ArchiveViewModel Archive { get; }

        /// <summary>
        /// The files
        /// </summary>
        public ObservableCollection<ArchiveFileViewModel> Files { get; }

        /// <summary>
        /// Indicates if the item is selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                // Only update value if deselected
                if (!value)
                {
                    _isSelected = false;
                    return;
                }

                // Get the previously selected directory
                var prevDir = Archive.SelectedItem;

                _isSelected = true;

                // Update the selected directory
                Task.Run(async () => await Archive.ChangeLoadedDirAsync(prevDir, this));
            }
        }

        /// <summary>
        /// Indicates if the item is expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// The name of the item to display
        /// </summary>
        public virtual string DisplayName => ID;

        /// <summary>
        /// The full directory path
        /// </summary>
        public string FullPath => FullID.JoinItems(Path.DirectorySeparatorChar.ToString());

        /// <summary>
        /// The current status to display
        /// </summary>
        public string DisplayStatus { get; set; } = String.Empty;

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

        public ICommand ImportCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new directory item to the view model
        /// </summary>
        /// <param name="dirName">The name of the directory. This is not the full relative path.</param>
        /// <returns>The item</returns>
        public ArchiveDirectoryViewModel Add(string dirName)
        {
            // Create the item
            var item = new ArchiveDirectoryViewModel(this, dirName, Archive);

            // Add the item
            Add(item);

            // Return the item
            return item;
        }

        /// <summary>
        /// Exports the directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportAsync()
        {
            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Run as a task
                await Task.Run(async () =>
                {
                    // Get the output path
                    var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        // TODO: Localize
                        Title = "Select destination to export to"
                    });

                    if (result.CanceledByUser)
                        return;

                    // Make sure the directory doesn't exist
                    if ((result.SelectedDirectory + DisplayName).DirectoryExists)
                    {
                        // TODO: Error message
                        return;
                    }

                    // TODO: Try/catch
                    // TODO: Progress bar

                    // Save the selected the format for each collection
                    Dictionary<string, int> selectedFormats = new Dictionary<string, int>();

                    // Select the format for each distinct collection
                    foreach (var formatGroup in this.GetAllChildren(true).SelectMany(x => x.Files).GroupBy(x => x.FileData.FileFormatName))
                    {
                        var data = formatGroup.First().FileData;

                        // TODO: Dialog for selecting file format for this file type

                        selectedFormats.Add(data.FileFormatName, 1);
                    }

                    // Handle each directory
                    foreach (var item in this.GetAllChildren(true))
                    {
                        // Get the directory path
                        var path = result.SelectedDirectory + DisplayName + item.FullPath.Remove(0, FullPath.Length).Trim(Path.DirectorySeparatorChar);

                        // Create the directory
                        Directory.CreateDirectory(path);

                        // Save each file
                        foreach (var file in item.Files)
                        {
                            // Get the selected format
                            var format = file.FileData.SupportedFileExtensions[selectedFormats[file.FileData.FileFormatName]];

                            // TODO: Localize
                            Archive.DisplayStatus = $"Exporting {file.FileName}";

                            // Save the file
                            await file.FileData.ExportFileAsync(file.ArchiveFileStream,
                                path + (new FileSystemPath(file.FileName).ChangeFileExtension(format)), format);
                        }
                    }

                    Archive.DisplayStatus = String.Empty;

                    // TODO: Success message
                });
            }
        }

        /// <summary>
        /// Imports files to the directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportAsync()
        {
            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Run as a task
                await Task.Run(async () =>
                {
                    // TODO: Try/catch
                    // TODO: Progress bar

                    // Get the directory
                    var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        // TODO: Localize
                        Title = "Select directory to import",
                    });

                    if (result.CanceledByUser)
                        return;

                    // Keep track of the number of files getting imported
                    var imported = 0;

                    // Keep track of if any files were not imported
                    var failes = false;

                    // Enumerate each directory view model
                    foreach (var dir in this.GetAllChildren(true))
                    {
                        // Enumerate each file
                        foreach (var file in dir.Files)
                        {
                            // Get the file path, without an extension, relative to the selected directory
                            FileSystemPath filePath = Path.Combine(result.SelectedDirectory, dir.FullPath.Remove(0, FullPath.Length), Path.GetFileNameWithoutExtension(file.FileName));

                            // Attempt to find a file for each supported extension
                            foreach (string ext in file.FileData.SupportedFileExtensions)
                            {
                                // Get the path
                                var fullFilePath = filePath.ChangeFileExtension(ext);

                                // Make sure the file exists
                                if (!fullFilePath.FileExists)
                                    continue;

                                // TODO: Localize
                                Archive.DisplayStatus = $"Importing {file.FileName}";

                                // Import the file
                                var succeeded = await file.FileData.ImportFileAsync(Archive.ArchiveFileStream, fullFilePath);

                                if (!succeeded)
                                    failes = true;

                                imported++;

                                // Break the loop
                                break;
                            }
                        }
                    }

                    // Make sure at least one file has been imported
                    if (imported == 0)
                    {
                        // TODO: Error message

                        return;
                    }

                    Archive.DisplayStatus = String.Empty;

                    if (failes)
                    {
                        // TODO: Show warning message that some failed to import
                    }

                    // Update the archive
                    await Archive.UpdateArchiveAsync();

                    // TODO: Success message
                });
            }
        }

        public virtual void Dispose()
        {
            // Disable collection synchronization
            BindingOperations.DisableCollectionSynchronization(Files);
            BindingOperations.DisableCollectionSynchronization(this);
        }

        #endregion
    }
}