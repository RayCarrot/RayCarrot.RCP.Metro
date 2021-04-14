using RayCarrot.Common;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using RayCarrot.IO;

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
            CreateDirectoryCommand = new AsyncRelayCommand(CreateDirectoryAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteDirectoryAsync);
            DeleteSelectedFileCommand = new AsyncRelayCommand(DeleteSelectedFileAsync);
            AddFilesCommand = new AsyncRelayCommand(AddFilesAsync);

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
            CreateDirectoryCommand = new AsyncRelayCommand(CreateDirectoryAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteDirectoryAsync);
            DeleteSelectedFileCommand = new AsyncRelayCommand(DeleteSelectedFileAsync);
            AddFilesCommand = new AsyncRelayCommand(AddFilesAsync);

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(Files, Application.Current);
            BindingOperations.EnableCollectionSynchronization(this, Application.Current);
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
        public string FullPath => Archive.Manager.CombinePaths(FullID.Skip(1));

        /// <summary>
        /// Indicates if the directory can be deleted
        /// </summary>
        public bool CanBeDeleted => Parent != null && Archive.Manager.CanModifyDirectories;

        /// <summary>
        /// Indicates if a sub-directory can be added
        /// </summary>
        public bool CanAddSubDirectory => Archive.Manager.CanModifyDirectories;

        #endregion

        #region Commands

        public ICommand ExtractCommand { get; }

        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }

        public ICommand CreateDirectoryCommand { get; }
        public ICommand DeleteCommand { get; }

        public ICommand DeleteSelectedFileCommand { get; }
        
        public ICommand AddFilesCommand { get; }

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
                    // Get the output path
                    var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        Title = Resources.Archive_ExportHeader
                    });

                    if (result.CanceledByUser)
                        return;

                    // Make sure there isn't an existing file at the output path
                    if ((result.SelectedDirectory + ExportDirName).FileExists)
                    {
                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Archive_ExportDirFileConflict, ExportDirName), MessageType.Error);

                        return;
                    }

                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // Get the manager
                        var manager = Archive.Manager;

                        // Save the selected format for each collection
                        Dictionary<IArchiveFileType, FileExtension> selectedFormats = new Dictionary<IArchiveFileType, FileExtension>();

                        try
                        {
                            // Handle each directory
                            foreach (var item in this.GetAllChildren(true))
                            {
                                // Get the directory path
                                var path = result.SelectedDirectory + ExportDirName + item.FullPath.Remove(0, FullPath.Length).Trim(manager.PathSeparatorCharacter);

                                // Create the directory
                                Directory.CreateDirectory(path);

                                // Save each file
                                foreach (var file in item.Files)
                                {
                                    // Get the file stream
                                    using var fileStream = file.GetDecodedFileStream();

                                    // Initialize the file without loading the thumbnail
                                    file.InitializeFile(fileStream, false);

                                    fileStream.SeekToBeginning();

                                    // Check if the format has not been selected
                                    if (!forceNativeFormat && !selectedFormats.ContainsKey(file.FileType) && !(file.FileType is ArchiveFileType_Default))
                                    {
                                        // Get the available extensions
                                        var ext = new string[]
                                        {
                                            Resources.Archive_Export_Format_Original
                                        }.Concat(file.FileType.ExportFormats.Select(x => x.FileExtensions)).ToArray();

                                        // Have user select the format
                                        FileExtensionSelectionDialogResult extResult = await RCPServices.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(ext, String.Format(Resources.Archive_FileExtensionSelectionInfoHeader, file.FileType.TypeDisplayName)));

                                        // Since this operation can't be canceled we get the first format
                                        if (extResult.CanceledByUser)
                                            extResult.SelectedFileFormat = ext.First();

                                        // Add the selected format
                                        selectedFormats.Add(file.FileType, extResult.SelectedFileFormat == ext.First() ? null : new FileExtension(extResult.SelectedFileFormat));
                                    }

                                    // Get the selected format
                                    var format = forceNativeFormat || (file.FileType is ArchiveFileType_Default) ? null : selectedFormats[file.FileType];

                                    // Get the final file name to use when exporting
                                    FileSystemPath exportFileName = forceNativeFormat ? new FileSystemPath(file.FileName) : new FileSystemPath(file.FileName).ChangeFileExtension(format, true);

                                    Archive.SetDisplayStatus(String.Format(Resources.Archive_ExportingFileStatus, file.FileName));

                                    // Export the file
                                    file.ExportFile(path + exportFileName, fileStream.Stream, format);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Exporting archive directory", DisplayName);

                            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, DisplayName));

                            return;
                        }
                        finally
                        {
                            Archive.SetDisplayStatus(String.Empty);
                        }

                        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ExportFilesSuccess);
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
                    // Get the directory
                    var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        Title = Resources.Archive_ImportDirectoryHeader,
                    });

                    if (result.CanceledByUser)
                        return;

                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // Keep track of the number of files getting imported
                        var imported = 0;

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

                                    if (!fileDir.DirectoryExists)
                                        continue;

                                    // Get the base file path
                                    var baseFilePath = fileDir + new FileSystemPath(file.FileName);

                                    // Get the file path, without an extension
                                    FileSystemPath filePath = baseFilePath.RemoveFileExtension(true);

                                    // Make sure there are potential file matches
                                    if (!Directory.GetFiles(fileDir, $"{filePath.Name}*", SearchOption.TopDirectoryOnly).Any())
                                        continue;

                                    // Get the file stream
                                    using var fileStream = file.GetDecodedFileStream();

                                    // Initialize the file without loading the thumbnail
                                    file.InitializeFile(fileStream, false);

                                    // Check if the base file exists without changing the extensions
                                    if (baseFilePath.FileExists)
                                    {
                                        // Import the file
                                        file.ImportFile(baseFilePath, false);

                                        imported++;

                                        continue;
                                    }

                                    // Attempt to find a file for each supported extension
                                    foreach (var ext in file.FileType.ImportFormats)
                                    {
                                        // Get the path
                                        var fullFilePath = filePath.ChangeFileExtension(ext);

                                        // Make sure the file exists
                                        if (!fullFilePath.FileExists)
                                            continue;

                                        // Import the file
                                        file.ImportFile(fullFilePath, true);

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

                            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ImportDir_Error);

                            return;
                        }

                        // Make sure at least one file has been imported
                        if (imported == 0)
                            await Services.MessageUI.DisplayMessageAsync(Resources.Archive_ImportNoFilesError, MessageType.Warning);
                    });
                }
            }
        }

        public async Task CreateDirectoryAsync()
        {
            if (!Archive.Manager.CanModifyDirectories)
                return;

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    var result = await RCPServices.UI.GetStringInput(new StringInputViewModel
                    {
                        Title = Resources.Archive_CreateDir_Header,
                        HeaderText = Resources.Archive_CreateDir_Header,
                        StringInput = "New Folder"
                    });

                    if (result.CanceledByUser)
                        return;

                    // Check if the name conflicts with an existing directory
                    if (this.Any(x => x.ID.Equals(result.StringInput, StringComparison.OrdinalIgnoreCase)))
                    {
                        RL.Logger?.LogTraceSource($"The archive directory {result.StringInput} was not added due to already existing");
                        return;
                    }

                    // Add the directory
                    Add(result.StringInput);

                    RL.Logger?.LogTraceSource($"The archive directory {result.StringInput} has been added to {DisplayName}");
                }
            }
        }

        /// <summary>
        /// Deletes the directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task DeleteDirectoryAsync()
        {
            // Make sure the archive supports creating directories
            if (!Archive.Manager.CanModifyDirectories)
                return;

            // We can't delete the root directory
            if (Archive.Parent == null)
                return;

            RL.Logger?.LogTraceSource($"The archive directory {DisplayName} is being removed...");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Remove from parent directory
                    Parent.Remove(this);

                    // Dispose the directory and the sub-directory
                    this.GetAllChildren(true).DisposeAll();

                    // Add as modified file
                    Archive.AddModifiedFiles();

                    RL.Logger?.LogTraceSource($"The archive directory has been removed");
                }
            }
        }

        public ArchiveFileViewModel GetSelectedFile() => Files.FirstOrDefault(x => x.IsSelected);

        public async Task DeleteSelectedFileAsync()
        {
            var selectedFile = GetSelectedFile();

            if (selectedFile != null)
                await selectedFile.DeleteFileAsync();
        }

        public async Task AddFilesAsync()
        {
            RL.Logger?.LogTraceSource($"Files are being added to {FullPath}");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Get the files
                    var result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                    {
                        Title = Resources.Archive_AddFiles_Header,
                        MultiSelection = true
                    });

                    if (result.CanceledByUser)
                        return;

                    // Add every file
                    await AddFilesAsync(result.SelectedFiles);
                }
            }
        }

        public async Task AddFilesAsync(IEnumerable<FileSystemPath> files)
        {
            // Get the manager
            var manager = Archive.Manager;

            var modifiedCount = 0;

            // Add every file
            foreach (var file in files)
            {
                var fileName = file.Name;
                var dir = FullPath;

                var existingFile = Files.FirstOrDefault(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                // Check if the file name conflicts with an existing file
                if (existingFile != null)
                {
                    if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Archive_AddFiles_Conflict, file), Resources.Archive_AddFiles_ConflictHeader, MessageType.Warning, true))
                        continue;
                }

                try
                {
                    // Open the file as a stream
                    using var fileStream = File.OpenRead(file);

                    var fileViewModel = existingFile ?? new ArchiveFileViewModel(new ArchiveFileItem(manager, fileName, dir, manager.GetNewFileEntry(Archive.ArchiveData, dir, fileName)), this);

                    // Replace the file with the import data
                    if (await Task.Run(() => fileViewModel.ReplaceFile(fileStream)))
                        modifiedCount++;

                    // Add the file to the list if it was created
                    if (existingFile == null)
                        Files.Add(fileViewModel);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Adding files to archive directory", DisplayName);

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_AddFiles_Error, file.Name));

                    return;
                }
            }

            Archive.AddModifiedFiles(modifiedCount);
        }

        /// <summary>
        /// Disposes the directory and its containing files
        /// </summary>
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