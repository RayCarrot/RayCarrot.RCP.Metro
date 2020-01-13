using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

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
        public bool IsSelected { get; set; }

        /// <summary>
        /// Indicates if the item is expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// The description to display
        /// </summary>
        public virtual string DisplayDescription => FullPath;

        /// <summary>
        /// The name of the item to display
        /// </summary>
        public virtual string DisplayName => ID;

        /// <summary>
        /// The full directory path
        /// </summary>
        public string FullPath => FullID.JoinItems(Path.DirectorySeparatorChar.ToString());

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
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // Get the output path
                        var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                        {
                            Title = Resources.Archive_ExportHeader
                        });

                        if (result.CanceledByUser)
                            return;

                        // Make sure the directory doesn't exist
                        if ((result.SelectedDirectory + DisplayName).DirectoryExists)
                        {
                            await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Archive_ExportDirectoryConflict, DisplayName), MessageType.Error);

                            return;
                        }

                        // Save the selected the format for each collection
                        Dictionary<string, string> selectedFormats = new Dictionary<string, string>();

                        // Select the format for each distinct collection
                        foreach (var formatGroup in this.GetAllChildren(true).SelectMany(x => x.Files).GroupBy(x => x.FileData.FileFormatName))
                        {
                            // Get the file data
                            var data = formatGroup.First().FileData;

                            // Have user select the format
                            FileExtensionSelectionDialogResult extResult = await RCFRCP.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(data.SupportedExportImportFileExtensions, String.Format(Resources.Archive_FileExtensionSelectionInfoHeader, data.FileFormatName)));

                            if (extResult.CanceledByUser)
                                return;

                            // Add the selected format
                            selectedFormats.Add(data.FileFormatName, extResult.SelectedFileFormat);
                        }

                        try
                        {
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
                                    var format = selectedFormats[file.FileData.FileFormatName];

                                    Archive.SetDisplayStatus(String.Format(Resources.Archive_ExportingFileStatus, file.FileName));

                                    // Save the file
                                    await file.FileData.ExportFileAsync(file.ArchiveFileStream,
                                        path + (new FileSystemPath(file.FileName).ChangeFileExtension(format, true)), format);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Exporting archive directory", DisplayName);

                            await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, DisplayName));

                            return;
                        }
                        finally
                        {
                            Archive.SetDisplayStatus(String.Empty);
                        }

                        await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ExportFilesSuccess);
                    });
                }
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
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // Get the directory
                        var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                        {
                            Title = Resources.Archive_ImportDirectoryHeader,
                        });

                        if (result.CanceledByUser)
                            return;

                        // Keep track of the number of files getting imported
                        var imported = 0;

                        // Keep track of if any files were not imported
                        var failes = false;

                        try
                        {
                            // Enumerate each directory view model
                            foreach (var dir in this.GetAllChildren(true))
                            {
                                // Enumerate each file
                                foreach (var file in dir.Files)
                                {
                                    // Get the file path, without an extension, relative to the selected directory
                                    FileSystemPath filePath = result.SelectedDirectory + dir.FullPath.Remove(0, FullPath.Length).Trim(Path.DirectorySeparatorChar) + (Path.GetFileNameWithoutExtension(file.FileName) ?? file.FileName);

                                    // Attempt to find a file for each supported extension
                                    foreach (string ext in file.FileData.SupportedImportFileExtensions)
                                    {
                                        // Get the path
                                        var fullFilePath = filePath.ChangeFileExtension(ext);

                                        // Make sure the file exists
                                        if (!fullFilePath.FileExists)
                                            continue;

                                        Archive.SetDisplayStatus(String.Format(Resources.Archive_ImportingFileStatus, file.FileName));

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
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Importing archive directory", DisplayName);

                            await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ImportError, result.SelectedDirectory.Name));

                            return;
                        }
                        finally
                        {
                            Archive.SetDisplayStatus(String.Empty);
                        }

                        // Make sure at least one file has been imported
                        if (imported == 0)
                        {
                            await RCFUI.MessageUI.DisplayMessageAsync(Resources.Archive_ImportNoFilesError, MessageType.Error);

                            return;
                        }

                        // Update the archive
                        var repackSucceeded = await Archive.UpdateArchiveAsync();

                        if (!repackSucceeded)
                            return;

                        // Check if any failed to import
                        if (failes)
                            await RCFUI.MessageUI.DisplayMessageAsync(Resources.Archive_ImportFailsError, MessageType.Warning);

                        await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ImportFilesSuccess);
                    });
                }
            }
        }

        public virtual void Dispose()
        {
            // Disable collection synchronization
            BindingOperations.DisableCollectionSynchronization(Files);
            BindingOperations.DisableCollectionSynchronization(this);

            // Dispose files
            Files.DisposeAll();
        }

        #endregion
    }
}