using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// View model for a directory in an archive
/// </summary>
[DebuggerDisplay("{" + nameof(FullPath) + "}")]
public class DirectoryViewModel : HierarchicalViewModel<DirectoryViewModel>, IArchiveFileSystemEntryViewModel
{
    #region Constructors

    /// <summary>
    /// Creates a directory item view model with the parent and directory name
    /// </summary>
    /// <param name="parent">The parent directory or null if it's the root</param>
    /// <param name="dirName">The directory name</param>
    /// <param name="archive">The archive the directory belongs to, or null if this is the archive</param>
    protected DirectoryViewModel(DirectoryViewModel? parent, string dirName, ArchiveViewModel? archive) : base(parent, dirName)
    {
        // Set the archive this directory belongs to
        Archive = archive 
                  ?? this as ArchiveViewModel 
                  ?? throw new ArgumentNullException(nameof(archive), $"Archive can't be null if not of type {nameof(ArchiveViewModel)}");

        // Create the file collection
        Files = new ObservableCollection<FileViewModel>();

        // Create commands
        ExportCommand = new AsyncRelayCommand(async () => await ExportAsync(false));
        ExtractCommand = new AsyncRelayCommand(async () => await ExportAsync(true));
        ImportCommand = new AsyncRelayCommand(ImportAsync);
        CreateDirectoryCommand = new AsyncRelayCommand(CreateDirectoryAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteDirectoryAsync);
        ExtractSelectedFilesCommand = new AsyncRelayCommand(async () => await ExportAsync(true, true));
        ExportSelectedFilesCommand = new AsyncRelayCommand(async () => await ExportAsync(false, true));
        DeleteSelectedFilesCommand = new AsyncRelayCommand(DeleteSelectedFilesAsync);
        RenameSelectedFileCommand = new AsyncRelayCommand(RenameSelectedFileAsync);
        AddFilesCommand = new AsyncRelayCommand(AddFilesAsync);

        // Enable collection synchronization
        Files.EnableCollectionSynchronization();
        this.EnableCollectionSynchronization();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The archive the directory belongs to
    /// </summary>
    public ArchiveViewModel Archive { get; }

    /// <summary>
    /// The files
    /// </summary>
    public ObservableCollection<FileViewModel> Files { get; }

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
    /// Indicates if the directory is the root directory
    /// </summary>
    public bool IsRoot => Parent == null;

    /// <summary>
    /// Indicates if the directory can be deleted
    /// </summary>
    public bool CanBeDeleted => Parent != null && Archive.Manager.CanModifyDirectories;

    /// <summary>
    /// Indicates if a sub-directory can be added
    /// </summary>
    public bool CanAddSubDirectory => Archive.Manager.CanModifyDirectories;

    /// <summary>
    /// The generic icon kind to use for the item
    /// </summary>
    public PackIconMaterialKind GenericIconKind => PackIconMaterialKind.FolderOutline;

    /// <summary>
    /// Indicates if the entry is a file, otherwise it's a directory or archive
    /// </summary>
    public bool IsFile => false;

    #endregion

    #region Commands

    public ICommand ExtractCommand { get; }

    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }

    public ICommand CreateDirectoryCommand { get; }
    public ICommand DeleteCommand { get; }

    public ICommand ExtractSelectedFilesCommand { get; }
    public ICommand ExportSelectedFilesCommand { get; }
    public ICommand DeleteSelectedFilesCommand { get; }
    public ICommand RenameSelectedFileCommand { get; }
        
    public ICommand AddFilesCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a new directory item to the view model
    /// </summary>
    /// <param name="dirName">The name of the directory. This is not the full relative path.</param>
    /// <returns>The item</returns>
    public DirectoryViewModel Add(string dirName)
    {
        // Create the item
        DirectoryViewModel item = new(this, dirName, Archive);

        // Add the item
        Add(item);

        // Return the item
        return item;
    }

    /// <summary>
    /// Exports the directory
    /// </summary>
    /// <param name="forceNativeFormat">Indicates if the native format should be forced</param>
    /// <param name="selectedFilesOnly">Indicates if only selected files in the current directory should be exported</param>
    /// <returns>The task</returns>
    public async Task ExportAsync(bool forceNativeFormat, bool selectedFilesOnly = false)
    {
        // Run as a load operation
        using (LoadState state = await Archive.LoaderViewModel.RunAsync(String.Format(Resources.Archive_ExportingFileStatus, DisplayName), canCancel: true))
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Get the output path
                DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
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
                    IArchiveDataManager manager = Archive.Manager;

                    // Save the selected format for each collection
                    Dictionary<IFileType, FileExtension?> selectedFormats = new();

                    try
                    {
                        DirectoryViewModel[] allDirs;
                        
                        if (selectedFilesOnly)
                            allDirs = new DirectoryViewModel[]
                            {
                                this
                            };
                        else
                            allDirs = this.GetAllChildren(true).ToArray();

                        int fileIndex = 0;
                        int filesCount = allDirs.SelectMany(x => x.Files).Count(x => !selectedFilesOnly || x.IsSelected);

                        // Handle each directory
                        foreach (DirectoryViewModel item in allDirs)
                        {
                            // Get the directory path
                            FileSystemPath path = result.SelectedDirectory + ExportDirName + item.FullPath.Remove(0, FullPath.Length).Trim(manager.PathSeparatorCharacter);

                            // Create the directory
                            Directory.CreateDirectory(path);

                            // Save each file
                            foreach (FileViewModel file in item.Files.Where(x => !selectedFilesOnly || x.IsSelected))
                            {
                                if (state.CancellationToken.IsCancellationRequested)
                                    return;

                                // Get the file stream
                                using ArchiveFileStream fileStream = file.GetDecodedFileStream();

                                // Initialize the file without loading the thumbnail
                                file.InitializeFile(fileStream, FileViewModel.ThumbnailLoadMode.None);

                                fileStream.SeekToBeginning();

                                // Check if the format has not been selected
                                if (!forceNativeFormat && !selectedFormats.ContainsKey(file.FileType) && file.FileType is not FileType_Default)
                                {
                                    // Get the available extensions
                                    string[] ext = new string[]
                                    {
                                        Resources.Archive_Export_Format_Original
                                    }.Concat(file.FileType.ExportFormats.Select(x => x.FileExtensions)).ToArray();

                                    // Have user select the format
                                    ItemSelectionDialogResult extResult = await Services.UI.SelectItemAsync(new ItemSelectionDialogViewModel(ext, String.Format(Resources.Archive_FileExtensionSelectionInfoHeader, file.FileType.TypeDisplayName)));

                                    // Since this operation can't be canceled we get the first format
                                    if (extResult.CanceledByUser)
                                        extResult.SelectedIndex = 0;

                                    // Add the selected format
                                    FileExtension? e = extResult.SelectedIndex == 0
                                        ? null
                                        : new FileExtension(ext[extResult.SelectedIndex], multiple: true);

                                    selectedFormats.Add(file.FileType, e);
                                }

                                // Get the selected format
                                FileExtension? format = forceNativeFormat || file.FileType is FileType_Default 
                                    ? null 
                                    : selectedFormats[file.FileType];

                                // Get the final file name to use when exporting
                                FileSystemPath exportFileName = format == null 
                                    ? new FileSystemPath(file.FileName) 
                                    : new FileSystemPath(file.FileName).ChangeFileExtension(format, true);

                                try
                                {
                                    // Export the file
                                    file.ExportFile(path + exportFileName, fileStream, format);
                                }
                                catch (Exception ex)
                                {
                                    // If the export failed for a native format we throw
                                    if (format == null)
                                        throw;

                                    Logger.Error(ex, "Exporting archive file {0}", file.FileName);

                                    // If the export failed and we tried converting it we instead export it as the native format
                                    // Start by setting the file in the error state, thus changing the type
                                    file.InitializeAsError();

                                    // Seek to the beginning of the stream in case some bytes were read
                                    fileStream.SeekToBeginning();

                                    // Export the file as the native format
                                    file.ExportFile(path + file.FileName, fileStream, null);
                                }

                                fileIndex++;
                                state.SetProgress(new Progress(fileIndex, filesCount));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Exporting archive directory {0}", DisplayName);

                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, DisplayName));

                        return;
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
        using (LoadState state = await Archive.LoaderViewModel.RunAsync(Resources.Archive_ImportDir_Status, canCancel: true))
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Get the directory
                DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                {
                    Title = Resources.Archive_ImportDirectoryHeader,
                });

                if (result.CanceledByUser)
                    return;

                // Run as a task
                await Task.Run(async () =>
                {
                    // Keep track of the number of files getting imported
                    int imported = 0;

                    try
                    {
                        DirectoryViewModel[] dirs = this.GetAllChildren(true).ToArray();
                        int filesCount = dirs.Sum(x => x.Files.Count);
                        int fileIndex = 0;

                        // Enumerate each directory view model
                        foreach (DirectoryViewModel dir in dirs)
                        {
                            // Enumerate each file
                            foreach (FileViewModel file in dir.Files)
                            {
                                if (state.CancellationToken.IsCancellationRequested)
                                    return;

                                state.SetProgress(new Progress(fileIndex, filesCount));
                                fileIndex++;

                                // Get the file directory, relative to the selected directory
                                FileSystemPath fileDir = result.SelectedDirectory + dir.FullPath.Remove(0, FullPath.Length).Trim(Archive.Manager.PathSeparatorCharacter);

                                if (!fileDir.DirectoryExists)
                                    continue;

                                // Get the base file path
                                FileSystemPath baseFilePath = fileDir + new FileSystemPath(file.FileName);

                                // Get the file path, without an extension
                                FileSystemPath filePath = baseFilePath.RemoveFileExtension(true);

                                // Make sure there are potential file matches
                                if (!Directory.GetFiles(fileDir, $"{filePath.Name}*", SearchOption.TopDirectoryOnly).Any())
                                    continue;

                                // Get the file stream
                                using ArchiveFileStream fileStream = file.GetDecodedFileStream();

                                // Initialize the file without loading the thumbnail
                                file.InitializeFile(fileStream, FileViewModel.ThumbnailLoadMode.None);

                                // Check if the base file exists without changing the extensions
                                if (baseFilePath.FileExists)
                                {
                                    // Import the file
                                    file.ImportFile(baseFilePath, false);

                                    imported++;

                                    continue;
                                }

                                // Attempt to find a file for each supported extension
                                foreach (FileExtension ext in file.FileType.ImportFormats)
                                {
                                    // Get the path
                                    FileSystemPath fullFilePath = filePath.ChangeFileExtension(ext);

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

                        state.SetProgress(new Progress(fileIndex, filesCount));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Importing archive directory {0}", DisplayName);

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
        using (await Archive.LoaderViewModel.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                StringInputResult result = await Services.UI.GetStringInputAsync(new StringInputViewModel
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
                    Logger.Trace("The archive directory {0} was not added due to already existing", result.StringInput);
                    return;
                }

                // Add the directory
                Add(result.StringInput);

                Logger.Trace("The archive directory {0} has been added to {1}", result.StringInput, DisplayName);
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
        {
            Logger.Trace("The directory {0} was not removed due to the manager not allowing directory modifications", DisplayName);
            return;
        }

        // We can't delete the root directory
        if (IsRoot)
            return;

        Logger.Trace("The archive directory {0} is being removed...", DisplayName);

        // Run as a load operation
        using (await Archive.LoaderViewModel.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Remove from parent directory
                Parent!.Remove(this);

                // Dispose the directory and the sub-directory
                this.GetAllChildren(true).DisposeAll();

                // Add as modified file
                Archive.AddModifiedFiles();

                Logger.Trace("The archive directory has been removed");
            }
        }
    }

    public IEnumerable<FileViewModel> GetSelectedFiles() => Files.Where(x => x.IsSelected);

    public async Task DeleteSelectedFilesAsync()
    {
        foreach (FileViewModel file in GetSelectedFiles().ToArray())
            await file.DeleteFileAsync();
    }

    public async Task RenameSelectedFileAsync()
    {
        FileViewModel? selectedFile = GetSelectedFiles().FirstOrDefault();

        if (selectedFile != null)
            await selectedFile.RenameFileAsync();
    }

    public async Task AddFilesAsync()
    {
        Logger.Trace("Files are being added to {0}", FullPath);

        // Run as a load operation
        using (LoadState state = await Archive.LoaderViewModel.RunAsync(Resources.Archive_AddFiles_Status, canCancel: true))
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Get the files
                FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                {
                    Title = Resources.Archive_AddFiles_Header,
                    MultiSelection = true
                });

                if (result.CanceledByUser)
                    return;

                try
                {
                    // Add every file
                    await AddFilesAsync(result.SelectedFiles, x => state.SetProgress(x), state.CancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Trace(ex, "Cancelled adding files to archive");
                }
            }
        }
    }

    public async Task AddFilesAsync(IEnumerable<FileSystemPath> files, Action<Progress> progressCallback, CancellationToken cancellationToken)
    {
        // Get the manager
        IArchiveDataManager manager = Archive.Manager;

        int modifiedCount = 0;
        int fileConflicts = 0;
        bool replaceConflicts = false;

        var addFiles = files.Select(x =>
        {
            FileViewModel? file = Files.FirstOrDefault(f => f.FileName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));

            if (file != null)
                fileConflicts++;

            return new
            {
                FilePath = x,
                ExistingFile = file,
            };
        }).ToArray();

        if (fileConflicts > 0)
        {
            string message = fileConflicts == 1
                ? String.Format(Resources.Archive_AddFiles_Conflict, addFiles.First(x => x.ExistingFile != null).FilePath.Name)
                : String.Format(Resources.Archive_AddFiles_ConflictMultiple, fileConflicts, 
                    String.Join(Environment.NewLine, addFiles.Where(x => x.ExistingFile != null).Take(10).Select(x => x.FilePath.Name)));

            if (fileConflicts > 10)
                message += $"{Environment.NewLine}...";

            replaceConflicts = await Services.MessageUI.DisplayMessageAsync(message, Resources.Archive_AddFiles_ConflictHeader, MessageType.Warning, true);
        }

        try
        {
            int fileIndex = 0;

            // Add every file
            foreach (var file in addFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string fileName = file.FilePath.Name;
                string dir = FullPath;
                FileViewModel? existingFile = file.ExistingFile;

                // Check if the file name conflicts with an existing file
                if (existingFile != null && !replaceConflicts)
                {
                    fileIndex++;
                    progressCallback(new Progress(fileIndex, addFiles.Length));
                    continue;
                }

                try
                {
                    // Open the file as a stream
                    using FileStream fileStream = File.OpenRead(file.FilePath);

                    FileViewModel fileViewModel = existingFile ?? new FileViewModel(new FileItem(manager, fileName, dir, manager.GetNewFileEntry(Archive.ArchiveData ?? throw new Exception("Archive data has not been loaded"), dir, fileName)), this);

                    // Replace the file with the import data
                    if (await Task.Run(() => fileViewModel.ReplaceFile(fileStream, new FileMetadata(file.FilePath),
                            // Always load the thumbnail if adding a new file and this directory is selected
                            forceLoadThumbnail: existingFile == null && IsSelected)))
                        modifiedCount++;

                    // Add the file to the list if it was created
                    if (existingFile == null)
                        Files.Add(fileViewModel);

                    fileIndex++;
                    progressCallback(new Progress(fileIndex, addFiles.Length));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Adding files to archive directory {0}", DisplayName);

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_AddFiles_Error, fileName));

                    return;
                }
            }
        }
        finally
        {
            Archive.AddModifiedFiles(modifiedCount);
            Archive.ExplorerDialogViewModel.RefreshStatusBar();
        }
    }

    /// <summary>
    /// Disposes the directory and its containing files
    /// </summary>
    public virtual void Dispose()
    {
        // Dispose files
        Files.DisposeAll();
    }

    #endregion
}