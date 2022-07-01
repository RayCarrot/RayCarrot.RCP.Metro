using MahApps.Metro.IconPacks;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// View model for a file in an archive
/// </summary>
[DebuggerDisplay("{" + nameof(FileName) + "}")]
public class FileViewModel : BaseViewModel, IDisposable, IArchiveFileSystemEntryViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="fileData">The file data</param>
    /// <param name="directory">The archive directory the file belongs to</param>
    public FileViewModel(FileItem fileData, DirectoryViewModel directory)
    {
        // Set properties
        FileData = fileData;
        Directory = directory;
        IconKind = PackIconMaterialKind.FileSyncOutline;
        FileDisplayInfo = new ObservableCollection<DuoGridItemViewModel>();
        FileExports = new ObservableCollection<ArchiveFileMenuActionViewModel>();
        EditActions = new ObservableCollection<ArchiveFileMenuActionViewModel>();
        FullFilePath = Manager.CombinePaths(FullPath, FileName);

        // Create commands
        ImportCommand = new AsyncRelayCommand(ImportFileAsync);
        ReplaceCommand = new AsyncRelayCommand(ReplaceFileAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteFileAsync);
        RenameCommand = new AsyncRelayCommand(RenameFileAsync);

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(FileExports, Application.Current);
        BindingOperations.EnableCollectionSynchronization(EditActions, Application.Current);
        BindingOperations.EnableCollectionSynchronization(FileDisplayInfo, Application.Current);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ImportCommand { get; }
    public ICommand ReplaceCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RenameCommand { get; }

    #endregion

    #region Private Fields

    #endregion

    #region Public Properties
        
    /// <summary>
    /// The archive the file belongs to
    /// </summary>
    public ArchiveViewModel Archive => Directory.Archive;

    /// <summary>
    /// The archive directory the file belongs to
    /// </summary>
    public DirectoryViewModel Directory { get; }

    /// <summary>
    /// The file data
    /// </summary>
    public FileItem FileData { get; }

    /// <summary>
    /// The image source for the thumbnail
    /// </summary>
    public ImageSource? ThumbnailSource { get; set; }

    /// <summary>
    /// The archive file stream
    /// </summary>
    public FileStream ArchiveFileStream => Archive.ArchiveFileStream;

    /// <summary>
    /// The archive data manager
    /// </summary>
    public IArchiveDataManager Manager => Archive.Manager;

    /// <summary>
    /// The name of the file
    /// </summary>
    public string FileName => FileData.FileName;

    /// <summary>
    /// The name of the item to display
    /// </summary>
    public string DisplayName => FileName;

    /// <summary>
    /// The full path for the file
    /// </summary>
    public string FullPath => Directory.FullPath;

    /// <summary>
    /// The full path for the file including the file name
    /// </summary>
    public string FullFilePath { get; }

    /// <summary>
    /// The info about the file to display
    /// </summary>
    public ObservableCollection<DuoGridItemViewModel> FileDisplayInfo { get; }

    /// <summary>
    /// The icon kind to use for the file
    /// </summary>
    public PackIconMaterialKind IconKind { get; set; }

    /// <summary>
    /// The generic icon kind to use for the item
    /// </summary>
    public PackIconMaterialKind GenericIconKind => PackIconMaterialKind.FileOutline;

    /// <summary>
    /// Indicates if the file is initialized
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Indicates if the file supports importing
    /// </summary>
    public bool CanImport { get; set; }

    /// <summary>
    /// The file export options for the file
    /// </summary>
    public ObservableCollection<ArchiveFileMenuActionViewModel> FileExports { get; }

    public ObservableCollection<ArchiveFileMenuActionViewModel> EditActions { get; }

    /// <summary>
    /// The file type (available after having been initialized once)
    /// </summary>
    public IFileType? FileType { get; set; }

    /// <summary>
    /// The file extension
    /// </summary>
    public FileExtension FileExtension => new(FileName, multiple: true);

    /// <summary>
    /// Indicates if the file has pending imports
    /// </summary>
    public bool HasPendingImport { get; set; }

    /// <summary>
    /// Indicates if the file is selected
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Indicates if the entry is a file, otherwise it's a directory or archive
    /// </summary>
    public bool IsFile => true;

    #endregion

    #region Private Methods

    private void LoadThumbnail(ArchiveFileStream fileStream, ThumbnailLoadMode thumbnailLoadMode = ThumbnailLoadMode.LoadThumbnail)
    {
        // If the mode is none we never want to load the thumbnail
        if (thumbnailLoadMode == ThumbnailLoadMode.None)
            return;

        // If the mod is set to only reload the thumbnail if already loaded then we return
        // if the thumbnail is not currently loaded. It might still be cached however, so
        // we remove it from the cache as well to ensure it gets reloaded next time.
        if (thumbnailLoadMode == ThumbnailLoadMode.ReloadThumbnailIfLoaded && ThumbnailSource == null)
        {
            Archive.ThumbnailCache.RemoveFromCache(this);
            return;
        }

        // If the thumbnail is set to be reloaded or if there is no already loaded thumbnail then we load a new one
        if (thumbnailLoadMode is ThumbnailLoadMode.ReloadThumbnailIfLoaded or ThumbnailLoadMode.ReloadThumbnail || 
            !Archive.ThumbnailCache.TryGetCachedItem(this, out FileThumbnailData? thumb))
        {
            fileStream.SeekToBeginning();

            if (FileType == null)
                throw new Exception("Can not load a thumbnail when the file type has not been set");

            // Load the thumbnail
            thumb = FileType.LoadThumbnail(fileStream, FileExtension, 64, Manager);

            // Cache the thumbnail
            Archive.ThumbnailCache.AddToCache(this, thumb);
        }

        // Get the thumbnail image
        ImageSource? img = thumb.Thumbnail;

        // Freeze the image to avoid thread errors
        img?.Freeze();

        // Set the image source
        ThumbnailSource = img;

        // Add display info from the type data
        FileDisplayInfo.AddRange(thumb.FileInfo);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the decoded file data in a stream
    /// </summary>
    /// <returns>The file stream with the decoded data</returns>
    public ArchiveFileStream GetDecodedFileStream() => FileData.GetDecodedFileData(Archive.ArchiveFileGenerator);

    [MemberNotNull(nameof(FileType))]
    public void SetFileType(IFileType type) => FileType = type;

    /// <summary>
    /// Initializes the file. This sets the <see cref="FileType"/> and optionally loads the <see cref="ThumbnailSource"/> and <see cref="FileDisplayInfo"/>.
    /// </summary>
    /// <param name="fileStream">The file stream, if available</param>
    /// <param name="thumbnailLoadMode">Indicates how the thumbnail should be loaded</param>
    [MemberNotNull(nameof(FileType))]
    public void InitializeFile(ArchiveFileStream? fileStream = null, ThumbnailLoadMode thumbnailLoadMode = ThumbnailLoadMode.LoadThumbnail)
    {
        bool hadStream = fileStream != null;

        try
        {
            // Get the file data
            fileStream ??= GetDecodedFileStream();

            // Populate info
            FileDisplayInfo.Clear();

            FileDisplayInfo.Add(new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Dir)), 
                text: FileData.Directory));

            FileDisplayInfo.AddRange(Manager.GetFileInfo(Archive.ArchiveData ?? throw new Exception("Archive data has not been loaded"), FileData.ArchiveEntry));

            fileStream.SeekToBeginning();

            // Get the type if we don't have one
            if (FileType == null)
                SetFileType(FileData.GetFileType(fileStream));

            ResetMenuActions();

            // Load the thumbnail
            LoadThumbnail(fileStream, thumbnailLoadMode);

            // Set icon
            IconKind = FileType!.Icon;

            // Set file type
            FileDisplayInfo.Add(new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Type)), 
                text: FileType.TypeDisplayName, 
                minUserLevel: UserLevel.Advanced));

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            // If the stream has closed it's not an error
            if (!ArchiveFileStream.CanRead)
            {
                Logger.Debug(ex, "Error initializing file");

                // Even though we've marked that FileType always gets set in this method there is a potential that it doesn't get
                // set if reaching this point while being null. This should never happen, but let's log as an error in case.
                if (FileType == null)
                {
                    Logger.Error($"The file type is null when exiting {nameof(InitializeFile)}!");
                    InitializeAsError();
                }
            }
            else
            {
                Logger.Error(ex, "Error initializing file of type {0}", FileType);

                // Initialize the file in the error state
                InitializeAsError();
            }
        }
        finally
        {
            if (!hadStream)
                fileStream?.Dispose();
        }
    }

    /// <summary>
    /// Initializes the file in the error state. This will allow the file to still be deleted and exported in its native format, but it
    /// can no longer be converted using its actual format.
    /// </summary>
    [MemberNotNull(nameof(FileType))]
    public void InitializeAsError()
    {
        SetFileType(new FileType_Error());
        ResetMenuActions();
        IconKind = FileType!.Icon;
        ThumbnailSource = null;
        IsInitialized = true;
        CanImport = false;
    }

    /// <summary>
    /// Unloads the file if it has been initialized. This does NOT dispose all resources the file uses. To fully dispose it, call <see cref="Dispose"/>. Unloading should be used when the file might be initialized again.
    /// </summary>
    public void Unload()
    {
        // Mark the file as not being initialized
        IsInitialized = false;

        // Unload the thumbnail
        ThumbnailSource = null;
            
        // Deselect the file (this makes sure that the selection is cleared when the file is loaded the next time)
        IsSelected = false;
    }

    protected void ResetMenuActions()
    {
        if (FileType == null)
            throw new Exception("The file type must be set before resetting menu actions");

        // Get formats from the type
        FileExtension[] importFormats = FileType.ImportFormats;
        FileExtension[] exportFormats = FileType.ExportFormats;

        // Remove previous export formats
        FileExports.Clear();

        // Add native export format
        FileExports.Add(new ArchiveFileMenuActionViewModel($"{Resources.Archive_Format_Original} ({FileExtension})", new AsyncRelayCommand(async () => await ExportFileAsync())));

        // Get export formats
        FileExports.AddRange(exportFormats.Select(x => new ArchiveFileMenuActionViewModel(x.DisplayName, new AsyncRelayCommand(async () => await ExportFileAsync(x)))));

        // Remove previous edit actions
        EditActions.Clear();

        // Add native and binary edit actions
        EditActions.Add(new ArchiveFileMenuActionViewModel($"{Resources.Archive_Format_Original} ({FileExtension})", new AsyncRelayCommand(async () => await EditFileAsync(null, false, false, false))));
        EditActions.Add(new ArchiveFileMenuActionViewModel(Resources.Archive_EditBinary, new AsyncRelayCommand(async () => await EditFileAsync(null, false, true, false))));

        // Get available formats to convert to/from
        EditActions.AddRange(exportFormats.Select(x =>
        {
            var readOnly = importFormats.All(f => x != f);

            var name = x.DisplayName;

            if (readOnly)
                name += $" ({Resources.ReadOnly})";

            return new ArchiveFileMenuActionViewModel(name, new AsyncRelayCommand(async () => await EditFileAsync(x, true, false, readOnly)));
        }));

        CanImport = importFormats.Any();
    }

    /// <summary>
    /// Exports the file using the specified format
    /// </summary>
    /// <param name="format">The format to export as</param>
    /// <returns>The task</returns>
    public async Task ExportFileAsync(FileExtension? format = null)
    {
        Logger.Trace("The archive file {0} is being exported as {1}", FileName, format?.FileExtensions ?? "original");

        // Run as a load operation
        using (await Archive.LoadOperation.RunAsync(String.Format(Resources.Archive_ExportingFileStatus, FileName)))
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Get the output path
                SaveFileResult result = await Services.BrowseUI.SaveFileAsync(format != null ? new SaveFileViewModel()
                {
                    Title = Resources.Archive_ExportHeader,
                    DefaultName = new FileSystemPath(FileName).ChangeFileExtension(format.GetPrimaryFileExtension(), true),
                    Extensions = format.GetFileFilterItem.ToString()
                } : new SaveFileViewModel()
                {
                    Title = Resources.Archive_ExportHeader,
                    DefaultName = FileName,
                    Extensions = FileExtension.GetFileFilterItem.ToString()
                });

                if (result.CanceledByUser)
                    return;

                // Run as a task
                await Task.Run(async () =>
                {
                    try
                    {
                        // Get the file data
                        using ArchiveFileStream fileStream = GetDecodedFileStream();

                        // Export the file
                        ExportFile(result.SelectedFileLocation, fileStream, format);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Exporting archive file {0}", FileName);

                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, FileName));

                        return;
                    }

                    Logger.Trace("The archive file has been exported");

                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ExportFileSuccess);
                });
            }
        }
    }

    /// <summary>
    /// Exports the specified file
    /// </summary>
    /// <param name="filePath">The file path to export to</param>
    /// <param name="stream">The file stream to export</param>
    /// <param name="format">The format to export as, or null to not convert it</param>
    /// <returns>The task</returns>
    public void ExportFile(FileSystemPath filePath, ArchiveFileStream stream, FileExtension? format)
    {
        Logger.Trace("An archive file is being exported as {0}", format);

        // Create the output file and open it
        using FileStream fileStream = File.Create(filePath);

        try
        {
            // Write the bytes directly to the stream if no format is specified
            if (format == null)
            {
                stream.Stream.CopyTo(fileStream);
            }
            // Convert the file if the format is not the native one
            else
            {
                if (FileType == null)
                    throw new Exception("The file type must be set before exporting the file with a format specified");

                FileType.ConvertTo(FileExtension, format, stream, fileStream, Manager);
            }
        }
        catch
        {
            // If writing to the file failed after it was created we delete the file
            fileStream.Close();
            Services.File.DeleteFile(filePath);

            // Throw the exception
            throw;
        }
    }

    /// <summary>
    /// Imports a file
    /// </summary>
    /// <returns>The task</returns>
    public async Task ImportFileAsync()
    {
        Logger.Trace("The archive file {0} is being imported...", FileName);

        if (FileType == null)
            throw new Exception("The file type must be set before importing the file");

        // Run as a load operation
        using (await Archive.LoadOperation.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Get the file
                FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                {
                    Title = Resources.Archive_ImportFileHeader,
                    ExtensionFilter = new FileFilterItemCollection(FileType.ImportFormats.Select(x => x.GetFileFilterItem)).CombineAll(Resources.Archive_FileSelectionGroupName).ToString()
                });

                if (result.CanceledByUser)
                    return;

                // Run as a task
                await Task.Run(async () =>
                {
                    try
                    {
                        // Import the file
                        ImportFile(result.SelectedFile, true);

                        Logger.Trace("The archive file is pending to be imported");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Importing file");

                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ImportFile_Error);
                    }
                });
            }
        }
    }

    /// <summary>
    /// Replaces the file with another file
    /// </summary>
    /// <returns>The task</returns>
    public async Task ReplaceFileAsync()
    {
        Logger.Trace("The archive file {0} is being replaced...", FileName);

        // Run as a load operation
        using (await Archive.LoadOperation.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Get the file
                FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                {
                    Title = Resources.Archive_ReplaceFileHeader
                });

                if (result.CanceledByUser)
                    return;

                // Run as a task
                await Task.Run(async () =>
                {
                    try
                    {
                        // Import the file
                        ImportFile(result.SelectedFile, false);

                        Logger.Trace("The archive file is pending to be imported");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Replacing file");

                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ReplaceFile_Error);
                    }
                });
            }
        }
    }

    public void ImportFile(FileSystemPath file, bool convert)
    {
        // Open the file to be imported
        using ArchiveFileStream importFile = new(File.OpenRead(file), file.Name, true);

        // Import the file
        ImportFile(importFile, file.FileExtensions, convert);
    }

    public void ImportFile(ArchiveFileStream importFile, FileExtension fileExtension, bool convert)
    {
        // Memory stream for converted data
        using MemoryStream memStream = new MemoryStream();

        // Convert from the imported file to the memory stream
        if (convert)
        {
            if (FileType == null)
                throw new Exception("The file type must be set before importing and converting the file");

            FileType.ConvertFrom(fileExtension, FileExtension, GetDecodedFileStream(), importFile, new ArchiveFileStream(memStream, FileName, false), Manager);
        }

        // Replace the file with the import data
        if (ReplaceFile(convert ? memStream : importFile.Stream))
            Archive.AddModifiedFiles();
    }

    /// <summary>
    /// Replaces the current file with the data from the stream
    /// </summary>
    /// <param name="inputStream">The decoded data stream</param>
    /// <returns>True if the file should be added as a new modified file, otherwise false</returns>
    public bool ReplaceFile(Stream inputStream)
    {
        bool wasModified = HasPendingImport;

        // Reset position
        inputStream.Position = 0;

        FileData.SetPendingImport();

        // Encode the data to the pending import stream
        Manager.EncodeFile(inputStream, FileData.PendingImport, FileData.ArchiveEntry);

        // If no data was encoded we copy over the original data
        if (FileData.PendingImport.Length == 0)
            inputStream.CopyTo(FileData.PendingImport);

        HasPendingImport = true;

        inputStream.Position = 0;
            
        // Initialize the file
        InitializeFile(new ArchiveFileStream(inputStream, FileName, false), ThumbnailLoadMode.ReloadThumbnailIfLoaded);

        return !wasModified;
    }

    /// <summary>
    /// Deletes the file from the archive
    /// </summary>
    /// <returns>The task</returns>
    public async Task DeleteFileAsync()
    {
        Logger.Trace("The archive file {0} is being removed...", FileName);

        // Run as a load operation
        using (await Archive.LoadOperation.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                DeleteFile();
            }
        }
    }

    public void DeleteFile()
    {
        // Remove the file from the directory
        Directory.Files.Remove(this);

        // Dispose the file
        Dispose();

        // Add as modified file
        Archive.AddModifiedFiles();

        Archive.ExplorerDialogViewModel.RefreshStatusBar();

        Logger.Trace("The archive file has been removed");
    }

    /// <summary>
    /// Opens the file for editing
    /// </summary>
    /// <param name="ext">The file extension to use when opening</param>
    /// <param name="convert">Indicates if the file should be converted when opening</param>
    /// <param name="asBinary">Indicates if the file should be opened as a binary file</param>
    /// <param name="readOnly">Indicates if the file should be opened as read-only</param>
    /// <returns>The task</returns>
    public async Task EditFileAsync(FileExtension? ext, bool convert, bool asBinary, bool readOnly)
    {
        Logger.Trace("The archive file {0} is being opened...", FileName);

        if (convert && asBinary)
            throw new Exception("A file can't be converted when opened as binary");

        if (convert && ext == null)
            throw new Exception("A file can't be converted with the original file extension");

        // Use the original file extension if none is specified
        ext ??= FileExtension;

        // Run as a load operation
        using (DisposableOperation operation = await Archive.LoadOperation.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                try
                {
                    // Get a temporary file
                    using TempFile tempFile = new(false, asBinary ? new FileExtension(".bin") : ext);

                    using HashAlgorithm sha1 = HashAlgorithm.Create();

                    IEnumerable<byte> originalHash;

                    // Decode the file data
                    using (var decodedData = GetDecodedFileStream())
                    {
                        // Create the temporary file
                        using FileStream temp = File.Create(tempFile.TempPath);

                        // Copy the file data to the temporary file
                        if (!convert)
                        {
                            decodedData.Stream.CopyTo(temp);
                        }
                        else
                        {
                            if (FileType == null)
                                throw new Exception("The file type must be set before editing and converting the file");

                            FileType.ConvertTo(FileExtension, ext, decodedData, temp, Manager);
                        }

                        temp.Position = 0;

                        // Get the original file hash
                        originalHash = sha1.ComputeHash(temp);
                    }

                    // Get the program to open the file with
                    FileSystemPath? programPath = null;

                    // If not opening as binary we check for programs associated with the file extension
                    if (!asBinary)
                    {
                        var extPrograms = Services.Data.Archive_AssociatedPrograms;

                        // Start by checking if the user has specified a default program
                        if (extPrograms.ContainsKey(ext.FileExtensions))
                        {
                            var exe = extPrograms[ext.FileExtensions];

                            if (exe.FileExists)
                                programPath = exe;
                            else
                                Services.Data.Archive_RemoveAssociatedProgram(ext);
                        }

                        // If not we try and get the registered default program
                        if (programPath == null)
                        {
                            var exe = WindowsHelpers.FindExecutableForFile(tempFile.TempPath, out uint? errorCode);

                            if (errorCode != null)
                            {
                                if (errorCode == 2)
                                    Logger.Warn("Executable was not found due to file not existing");
                                else if (errorCode == 3)
                                    Logger.Warn("Executable was not found due to the path being invalid");
                                else if (errorCode == 5)
                                    Logger.Warn("Executable was not found due to that the file could not be accessed");
                                else if (errorCode == 8)
                                    Logger.Warn("Executable was not found due to the system being out of memory");
                                else if (errorCode == 31)
                                    Logger.Warn("Executable was not found due to there not being an association for the specified file type with an executable file");
                                else
                                    Logger.Warn("Executable was not found due to an unknown error");
                            }

                            if (exe != null)
                                programPath = exe;
                        }
                    }
                    else
                    {
                        var binaryEditor = Services.Data.Archive_BinaryEditorExe;

                        if (binaryEditor.FileExists)
                            programPath = binaryEditor;
                    }

                    // If it's still null we ask the user for the program to use
                    if (programPath == null)
                    {
                        string e = asBinary ? Resources.Archive_EditBinary.ToLower() : $"{ext}";

                        ProgramSelectionResult programResult = await Services.UI.GetProgramAsync(new ProgramSelectionViewModel()
                        {
                            Title = String.Format(Resources.Archive_SelectEditExe, e),
                            FileExtensions = asBinary ? null : new FileExtension[] { ext },
                        });

                        if (programResult.CanceledByUser)
                            return;

                        if (asBinary)
                            Services.Data.Archive_BinaryEditorExe = programResult.ProgramFilePath;
                        else
                            Services.Data.Archive_AddAssociatedProgram(ext, programResult.ProgramFilePath);

                        programPath = programResult.ProgramFilePath;
                    }

                    // If read-only set the attribute
                    if (readOnly)
                    {
                        var info = tempFile.TempPath.GetFileInfo();
                        info.Attributes |= FileAttributes.ReadOnly;
                    }

                    // Open the file
                    using (var p = await Services.File.LaunchFileAsync(programPath.Value, arguments: $"\"{tempFile.TempPath}\""))
                    {
                        // Ignore if the file wasn't opened
                        if (p == null)
                        {
                            Logger.Trace("The file was not opened");
                            return;
                        }

                        operation.SetText(String.Format(Resources.WaitForEditorToClose, programPath.Value.RemoveFileExtension().Name));

                        // Wait for the file to close...
                        await p.WaitForExitAsync();
                     
                        operation.SetText(String.Empty);
                    }

                    // If read-only we don't need to check if it has been modified
                    if (readOnly)
                        return;

                    // Open the temp file
                    using ArchiveFileStream tempFileStream = new(new FileStream(tempFile.TempPath, FileMode.Open, FileAccess.Read), "Temp", true);

                    // Get the new hash
                    var newHash = sha1.ComputeHash(tempFileStream.Stream);

                    tempFileStream.SeekToBeginning();

                    // Check if the file has been modified
                    if (!originalHash.SequenceEqual(newHash))
                    {
                        Logger.Trace("The file was modified");

                        // Import the modified file
                        ImportFile(tempFileStream, ext, convert);
                    }
                    else
                    {
                        Logger.Trace("The file was not modified");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Opening archive file for editing");

                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ViewEditFileError);
                }
            }
        }
    }

    public async Task RenameFileAsync()
    {
        if (FileType == null)
            throw new Exception("The file type must be set before the file can be renamed");

        // Run as a load operation
        using (await Archive.LoadOperation.RunAsync())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                StringInputResult result = await Services.UI.GetStringInputAsync(new StringInputViewModel
                {
                    Title = Resources.Archive_SetFileName,
                    HeaderText = Resources.Archive_SetFileName,
                    StringInput = FileName
                });

                if (result.CanceledByUser)
                    return;

                string newName = result.StringInput;
                DirectoryViewModel dir = Directory;

                // Check if the name conflicts with an existing file
                if (Directory.Files.Any(x => x.FileName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    await Services.MessageUI.DisplayMessageAsync(Resources.Archive_FileNameConflict, Resources.Archive_AddFiles_ConflictHeader, MessageType.Error);
                    return;
                }

                // Create a new file
                FileViewModel newFile = new(new FileItem(Manager, newName, dir.FullPath, Manager.GetNewFileEntry(Archive.ArchiveData ?? throw new Exception("Archive data has not been loaded"), dir.FullPath, newName)), dir);

                // Set the file type
                newFile.SetFileType(FileType);

                // Copy the file contents
                newFile.ReplaceFile(GetDecodedFileStream().Stream);

                // Add the new file
                dir.Files.Insert(dir.Files.IndexOf(this), newFile);

                // Delete this file
                DeleteFile();

                Logger.Trace("The file {0} was renamed to {1}", FileName, newName);
            }
        }
    }

    /// <summary>
    /// Disposes the file
    /// </summary>
    public void Dispose()
    {
        // Disable collection synchronization
        BindingOperations.DisableCollectionSynchronization(FileExports);
        BindingOperations.DisableCollectionSynchronization(EditActions);
        BindingOperations.DisableCollectionSynchronization(FileDisplayInfo);

        // Dispose the file data
        FileData.Dispose();
    }

    #endregion

    #region Data Types

    /// <summary>
    /// The mode to use when loading a thumbnail
    /// </summary>
    public enum ThumbnailLoadMode
    {
        /// <summary>
        /// Don't load the thumbnail
        /// </summary>
        None,

        /// <summary>
        /// Loads the thumbnail, unless already loaded
        /// </summary>
        LoadThumbnail,

        /// <summary>
        /// Force reload the thumbnail even if already loaded
        /// </summary>
        ReloadThumbnail,

        /// <summary>
        /// Reloads the thumbnail if it has already been loaded
        /// </summary>
        ReloadThumbnailIfLoaded,
    }

    /// <summary>
    /// View model for an archive file menu action
    /// </summary>
    public class ArchiveFileMenuActionViewModel : BaseViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="displayName">The action display name</param>
        /// <param name="menuCommand">The menu command</param>
        public ArchiveFileMenuActionViewModel(string displayName, ICommand menuCommand)
        {
            DisplayName = displayName;
            MenuCommand = menuCommand;
        }

        /// <summary>
        /// The action display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The menu command
        /// </summary>
        public ICommand MenuCommand { get; }
    }

    #endregion
}