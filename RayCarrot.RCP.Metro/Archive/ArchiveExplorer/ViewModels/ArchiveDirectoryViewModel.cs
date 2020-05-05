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
            ExportCommand = new AsyncRelayCommand(async () => await ExportAsync(false));
            ExtractCommand = new AsyncRelayCommand(async () => await ExportAsync(true));
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
            ExportCommand = new AsyncRelayCommand(async () => await ExportAsync(false));
            ExtractCommand = new AsyncRelayCommand(async () => await ExportAsync(true));
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
        /// The name of the directory to use when exporting
        /// </summary>
        public virtual string ExportDirName => DisplayName;

        /// <summary>
        /// The full directory path
        /// </summary>
        public string FullPath => FullID.JoinItems(Path.DirectorySeparatorChar.ToString());

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

        public ICommand ExtractCommand { get; }

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
        /// <param name="forceNativeFormat">Indicates if the native format should be forced</param>
        /// <returns>The task</returns>
        public async Task ExportAsync(bool forceNativeFormat)
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
                        if ((result.SelectedDirectory + ExportDirName).Exists)
                        {
                            await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Archive_ExportDirectoryConflict, ExportDirName), MessageType.Error);

                            return;
                        }

                        // Save the selected the format for each collection
                        Dictionary<string, FileExtension> selectedFormats = new Dictionary<string, FileExtension>();

                        try
                        {
                            // Handle each directory
                            foreach (var item in this.GetAllChildren(true))
                            {
                                // Get the directory path
                                var path = result.SelectedDirectory + ExportDirName + item.FullPath.Remove(0, FullPath.Length).Trim(Path.DirectorySeparatorChar);

                                // Create the directory
                                Directory.CreateDirectory(path);

                                // Save each file
                                foreach (var file in item.Files)
                                {
                                    // Get the file data
                                    var data = file.FileData;

                                    // Get the file bytes
                                    var bytes = data.GetDecodedFileBytes(file.ArchiveFileStream, Archive.ArchiveFileGenerator, false);

                                    // Check if the format has not been selected
                                    if (!forceNativeFormat && !selectedFormats.ContainsKey(data.FileFormatName))
                                    {
                                        // Get the available extensions
                                        var ext = data.SupportedExportFileExtensions.Select(x => x.FileExtensions).ToArray();

                                        // Have user select the format
                                        FileExtensionSelectionDialogResult extResult = await RCFRCP.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(ext, String.Format(Resources.Archive_FileExtensionSelectionInfoHeader, data.FileFormatName)));

                                        // Since this operation can't be canceled we get the first format
                                        if (extResult.CanceledByUser)
                                            extResult.SelectedFileFormat = ext.First();

                                        // Add the selected format
                                        selectedFormats.Add(data.FileFormatName, new FileExtension(extResult.SelectedFileFormat));
                                    }

                                    // Get the selected format
                                    var format = forceNativeFormat ? file.NativeFileExtension : selectedFormats[data.FileFormatName];

                                    // Get the final file name to use when exporting
                                    FileSystemPath exportFileName = forceNativeFormat ? new FileSystemPath(file.FileName) : new FileSystemPath(file.FileName).ChangeFileExtension(format, true);

                                    Archive.SetDisplayStatus(String.Format(Resources.Archive_ExportingFileStatus, file.FileName));

                                    // Export the file
                                    file.ExportFile(path + exportFileName, bytes, format);
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

                        // Get the import data
                        var importData = new List<ArchiveImportData>();

                        try
                        {
                            // Enumerate each directory view model
                            foreach (var dir in this.GetAllChildren(true))
                            {
                                // Enumerate each file
                                foreach (var file in dir.Files)
                                {
                                    // Get the file directory, relative to the selected directory
                                    FileSystemPath fileDir = result.SelectedDirectory + dir.FullPath.Remove(0, FullPath.Length).Trim(Path.DirectorySeparatorChar);

                                    // Get the base file path
                                    var baseFilePath = fileDir + new FileSystemPath(file.FileName);

                                    // Get the file path, without an extension
                                    FileSystemPath filePath = baseFilePath.RemoveFileExtension(true);

                                    if (!fileDir.DirectoryExists)
                                        continue;

                                    // Make sure there are potential file matches
                                    if (!Directory.GetFiles(fileDir, $"{filePath.Name}*", SearchOption.TopDirectoryOnly).Any())
                                        continue;

                                    // Initialize the file bytes
                                    file.FileData.GetDecodedFileBytes(Archive.ArchiveFileStream, Archive.ArchiveFileGenerator, true);

                                    // Check if the base file exists without changing the extensions
                                    if (baseFilePath.FileExists)
                                    {
                                        // Import the file
                                        ImportFile(baseFilePath);

                                        continue;
                                    }

                                    // Attempt to find a file for each supported extension
                                    foreach (var ext in file.FileData.SupportedImportFileExtensions)
                                    {
                                        // Get the path
                                        var fullFilePath = filePath.ChangeFileExtension(ext);

                                        // Make sure the file exists
                                        if (!fullFilePath.FileExists)
                                            continue;

                                        // Import the file
                                        ImportFile(fullFilePath);

                                        // Break the loop
                                        break;
                                    }

                                    // Helper method for importing a file
                                    void ImportFile(FileSystemPath fullFilePath)
                                    {
                                        // Import the file
                                        var data = file.ImportFile(fullFilePath);

                                        // Add the data to the collection
                                        importData.Add(data);

                                        imported++;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Importing archive directory", DisplayName);

                            await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_RepackError);

                            return;
                        }

                        // Make sure at least one file has been imported
                        if (imported == 0)
                        {
                            await RCFUI.MessageUI.DisplayMessageAsync(Resources.Archive_ImportNoFilesError, MessageType.Error);

                            return;
                        }

                        // Update the archive
                        var repackSucceeded = await Archive.UpdateArchiveAsync(importData);

                        if (!repackSucceeded)
                            return;

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
        }

        #endregion
    }
}