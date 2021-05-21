using MahApps.Metro.IconPacks;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a file in an archive
    /// </summary>
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    public class ArchiveFileViewModel : BaseViewModel, IDisposable, IArchiveExplorerEntryViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="archiveDirectory">The archive directory the file belongs to</param>
        public ArchiveFileViewModel(ArchiveFileItem fileData, ArchiveDirectoryViewModel archiveDirectory)
        {
            // Set properties
            FileData = fileData;
            ArchiveDirectory = archiveDirectory;
            IconKind = PackIconMaterialKind.FileSyncOutline;
            FileDisplayInfo = new ObservableCollection<DuoGridItemViewModel>();
            FileExports = new ObservableCollection<ArchiveFileMenuActionViewModel>();
            EditActions = new ObservableCollection<ArchiveFileMenuActionViewModel>();

            // Create commands
            ImportCommand = new AsyncRelayCommand(ImportFileAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteFileAsync);
            RenameCommand = new AsyncRelayCommand(RenameFileAsync);

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(FileExports, Application.Current);
            BindingOperations.EnableCollectionSynchronization(EditActions, Application.Current);
            BindingOperations.EnableCollectionSynchronization(FileDisplayInfo, Application.Current);
        }

        #endregion

        #region Commands

        public ICommand ImportCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RenameCommand { get; }

        #endregion

        #region Private Fields

        private bool _isInitialized;

        #endregion

        #region Public Properties

        /// <summary>
        /// The archive the file belongs to
        /// </summary>
        public ArchiveViewModel Archive => ArchiveDirectory.Archive;

        /// <summary>
        /// The archive directory the file belongs to
        /// </summary>
        public ArchiveDirectoryViewModel ArchiveDirectory { get; }

        /// <summary>
        /// The file data
        /// </summary>
        public ArchiveFileItem FileData { get; }

        /// <summary>
        /// The image source for the thumbnail
        /// </summary>
        public ImageSource ThumbnailSource { get; set; }

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
        public string FullPath => ArchiveDirectory.FullPath;

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
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                CanImport = FileType?.ImportFormats?.Any() == true;
            }
        }

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
        public IArchiveFileType FileType { get; set; }

        /// <summary>
        /// The file extension
        /// </summary>
        public FileExtension FileExtension => new FileExtension(FileName);

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

        #region Public Methods

        /// <summary>
        /// Gets the decoded file data in a stream
        /// </summary>
        /// <returns>The file stream with the decoded data</returns>
        public ArchiveFileStream GetDecodedFileStream()
        {
            ArchiveFileStream encodedStream = null;
            ArchiveFileStream decodedStream = null;

            try
            {
                // Get the encoded file bytes
                encodedStream = FileData.GetFileData(Archive.ArchiveFileGenerator);

                // Create a stream for the decoded bytes
                decodedStream = new ArchiveFileStream(new MemoryStream(), true);

                // Decode the bytes
                Manager.DecodeFile(encodedStream.Stream, decodedStream.Stream, FileData.ArchiveEntry);

                // Check if the data was decoded
                if (decodedStream.Stream.Length > 0)
                {
                    encodedStream?.Dispose();
                    decodedStream.SeekToBeginning();
                    return decodedStream;
                }
                else
                {
                    decodedStream?.Dispose();
                    encodedStream.SeekToBeginning();
                    return encodedStream;
                }
            }
            catch (Exception ex)
            {
                // Dispose both streams if an exception is thrown
                encodedStream?.Dispose();
                decodedStream?.Dispose();

                ex.HandleError("Getting decoded archive file data");

                throw;
            }
        }

        public void SetFileType(IArchiveFileType type) => FileType = type;

        /// <summary>
        /// Initializes the file. This sets the <see cref="FileType"/> and optionally loads the <see cref="ThumbnailSource"/> and <see cref="FileDisplayInfo"/>.
        /// </summary>
        /// <param name="fileStream">The file stream, if available</param>
        /// <param name="loadThumbnail">Indicates if the thumbnail should be loaded</param>
        public void InitializeFile(ArchiveFileStream fileStream = null, bool loadThumbnail = true)
        {
            var hadStream = fileStream != null;

            try
            {
                // Get the file data
                fileStream ??= GetDecodedFileStream();

                // Populate info
                FileDisplayInfo.Clear();

                FileDisplayInfo.Add(new DuoGridItemViewModel(Resources.Archive_FileInfo_Dir, FileData.Directory));

                FileDisplayInfo.AddRange(Manager.GetFileInfo(Archive.ArchiveData, FileData.ArchiveEntry));

                fileStream.SeekToBeginning();

                // Get the type if we don't have one
                if (FileType == null)
                    SetFileType(FileData.GetFileType(fileStream));

                ResetMenuActions();

                if (loadThumbnail)
                {
                    fileStream.SeekToBeginning();

                    // Load the thumbnail
                    (ImageSource img, var duoGridItemViewModels) = FileType.LoadThumbnail(fileStream, FileExtension, 64, Manager);

                    // Freeze the image to avoid thread errors
                    img?.Freeze();

                    // Set the image source
                    ThumbnailSource = img;

                    // Add display info from the type data
                    FileDisplayInfo.AddRange(duoGridItemViewModels);
                }

                // Set icon
                IconKind = FileType.Icon;

                // Set file type
                FileDisplayInfo.Add(new DuoGridItemViewModel(Resources.Archive_FileInfo_Type, FileType.TypeDisplayName, UserLevel.Advanced));

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                // If the stream has closed it's not an error
                if (!ArchiveFileStream.CanRead)
                {
                    ex.HandleExpected("Initializing file");
                }
                else
                {
                    ex.HandleError("Initializing file");

                    // Initialize the file with default settings to allow the file to be exported and deleted
                    SetFileType(new ArchiveFileType_Default());
                    ResetMenuActions();
                    IconKind = PackIconMaterialKind.FileAlertOutline;
                    ThumbnailSource = null;
                    IsInitialized = true;
                }
            }
            finally
            {
                if (!hadStream)
                    fileStream?.Dispose();
            }
        }

        /// <summary>
        /// Unloads the file if it has been initialized. This does NOT dispose all resources the file uses. To fully dispose it, call <see cref="Dispose"/>. Unloading should be used when the file might be initialized again.
        /// </summary>
        public void Unload()
        {
            // Mark the file as not being initialized
            IsInitialized = false;

            // TODO-UPDATE: Implement caching with max size to avoid it always getting reloaded?
            // Unload the thumbnail
            ThumbnailSource = null;
            
            // Deselect the file (this makes sure that the selection is cleared when the file is loaded the next time)
            IsSelected = false;
        }

        protected void ResetMenuActions()
        {
            // Get formats from the type
            var importFormats = FileType.ImportFormats;
            var exportFormats = FileType.ExportFormats;

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
        }

        /// <summary>
        /// Exports the file using the specified format
        /// </summary>
        /// <param name="format">The format to export as</param>
        /// <returns>The task</returns>
        public async Task ExportFileAsync(FileExtension format = null)
        {
            RL.Logger?.LogTraceSource($"The archive file {FileName} is being exported as {format?.FileExtensions ?? "original"}");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Get the output path
                    var result = await Services.BrowseUI.SaveFileAsync(format != null ? new SaveFileViewModel()
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
                        Archive.SetDisplayStatus(String.Format(Resources.Archive_ExportingFileStatus, FileName));

                        try
                        {
                            // Get the file data
                            using var fileStream = GetDecodedFileStream();

                            // Export the file
                            ExportFile(result.SelectedFileLocation, fileStream.Stream, format);
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Exporting archive file", FileName);

                            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, FileName));

                            return;
                        }
                        finally
                        {
                            Archive.SetDisplayStatus(String.Empty);
                        }

                        RL.Logger?.LogTraceSource($"The archive file has been exported");

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
        public void ExportFile(FileSystemPath filePath, Stream stream, FileExtension format)
        {
            RL.Logger?.LogTraceSource($"An archive file is being exported as {format}");

            // Create the output file and open it
            using var fileStream = File.Create(filePath);

            // Write the bytes directly to the stream if no format is specified
            if (format == null)
                stream.CopyTo(fileStream);
            // Convert the file if the format is not the native one
            else
                FileType.ConvertTo(FileExtension, format, stream, fileStream, Manager);
        }

        /// <summary>
        /// Imports a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportFileAsync()
        {
            RL.Logger?.LogTraceSource($"The archive file {FileName} is being imported...");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Get the file
                    var result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
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

                            RL.Logger?.LogTraceSource($"The archive file is pending to be imported");
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Importing file");

                            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ImportFile_Error);
                        }
                    });
                }
            }
        }

        public void ImportFile(FileSystemPath file, bool convert)
        {
            // Open the file to be imported
            using var importFile = File.OpenRead(file);

            // Import the file
            ImportFile(importFile, file.FileExtension, convert);
        }

        public void ImportFile(Stream importFile, FileExtension fileExtension, bool convert)
        {
            // Memory stream for converted data
            using var memStream = new MemoryStream();

            // Convert from the imported file to the memory stream
            if (convert)
                FileType.ConvertFrom(fileExtension, FileExtension, GetDecodedFileStream(), importFile, memStream, Manager);

            // Replace the file with the import data
            if (ReplaceFile(convert ? memStream : importFile))
                Archive.AddModifiedFiles();
        }

        /// <summary>
        /// Replaces the current file with the data from the stream
        /// </summary>
        /// <param name="inputStream">The decoded data stream</param>
        /// <returns>True if the file should be added as a new modified file, otherwise false</returns>
        public bool ReplaceFile(Stream inputStream)
        {
            var wasModified = HasPendingImport;

            // Reset position
            inputStream.Position = 0;

            // Get the temp stream to store the pending import data
            FileData.SetPendingImport(File.Create(Path.GetTempFileName()));

            // Encode the data to the pending import stream
            Manager.EncodeFile(inputStream, FileData.PendingImport, FileData.ArchiveEntry);

            // If no data was encoded we copy over the original data
            if (FileData.PendingImport.Length == 0)
                inputStream.CopyTo(FileData.PendingImport);

            HasPendingImport = true;

            inputStream.Position = 0;
            
            // Initialize the file
            InitializeFile(new ArchiveFileStream(inputStream, false));

            return !wasModified;
        }

        /// <summary>
        /// Deletes the file from the archive
        /// </summary>
        /// <returns>The task</returns>
        public async Task DeleteFileAsync()
        {
            RL.Logger?.LogTraceSource($"The archive file {FileName} is being removed...");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
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
            ArchiveDirectory.Files.Remove(this);

            // Dispose the file
            Dispose();

            // Add as modified file
            Archive.AddModifiedFiles();

            Archive.ExplorerDialogViewModel.RefreshStatusBar();

            RL.Logger?.LogTraceSource($"The archive file has been removed");
        }

        /// <summary>
        /// Opens the file for editing
        /// </summary>
        /// <param name="ext">The file extension to use when opening</param>
        /// <param name="convert">Indicates if the file should be converted when opening</param>
        /// <param name="asBinary">Indicates if the file should be opened as a binary file</param>
        /// <param name="readOnly">Indicates if the file should be opened as read-only</param>
        /// <returns>The task</returns>
        public async Task EditFileAsync(FileExtension ext, bool convert, bool asBinary, bool readOnly)
        {
            RL.Logger?.LogTraceSource($"The archive file {FileName} is being opened...");

            if (convert && asBinary)
                throw new Exception("A file can't be converted when opened as binary");

            if (convert && ext == null)
                throw new Exception("A file can't be converted with the original file extension");

            // Use the original file extension if none is specified
            ext ??= FileExtension;

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    try
                    {
                        // Get a temporary file
                        using var tempFile = new TempFile(asBinary ? new FileExtension(".bin") : ext);

                        using HashAlgorithm sha1 = HashAlgorithm.Create();

                        IEnumerable<byte> originalHash;

                        // Decode the file data
                        using (var decodedData = GetDecodedFileStream())
                        {
                            // Create the temporary file
                            using var temp = File.Create(tempFile.TempPath);

                            // Copy the file data to the temporary file
                            if (!convert)
                                decodedData.Stream.CopyTo(temp);
                            else
                                FileType.ConvertTo(FileExtension, ext, decodedData.Stream, temp, Manager);

                            temp.Position = 0;

                            // Get the original file hash
                            originalHash = sha1.ComputeHash(temp);
                        }

                        // Get the program to open the file with
                        FileSystemPath? programPath = null;

                        // If not opening as binary we check for programs associated with the file extension
                        if (!asBinary)
                        {
                            var extPrograms = RCPServices.Data.Archive_AssociatedPrograms;

                            // Start by checking if the user has specified a default program
                            if (extPrograms.ContainsKey(ext.FileExtensions))
                            {
                                var exe = extPrograms[ext.FileExtensions];

                                if (exe.FileExists)
                                    programPath = exe;
                                else
                                    RCPServices.Data.RemoveAssociatedProgram(ext);
                            }

                            // If not we try and get the registered default program
                            if (programPath == null)
                            {
                                var exe = WindowsHelpers.FindExecutableForFile(tempFile.TempPath);

                                if (exe != null)
                                    programPath = exe;
                            }
                        }
                        else
                        {
                            var binaryEditor = RCPServices.Data.Archive_BinaryEditorExe;

                            if (binaryEditor.FileExists)
                                programPath = binaryEditor;
                        }

                        // If it's still null we ask the user for the program to use
                        if (programPath == null)
                        {
                            var e = asBinary ? Resources.Archive_EditBinary.ToLower() : $"{ext}";

                            var browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
                            {
                                Title = String.Format(Resources.Archive_SelectEditExe, e),
                                ExtensionFilter = new FileFilterItem("*.exe", "Exe").StringRepresentation,
                                DefaultDirectory = Environment.SpecialFolder.ProgramFiles.GetFolderPath()
                            });

                            if (browseResult.CanceledByUser)
                                return;

                            if (asBinary)
                                RCPServices.Data.Archive_BinaryEditorExe = browseResult.SelectedFile;
                            else
                                RCPServices.Data.AddAssociatedProgram(ext, browseResult.SelectedFile);

                            programPath = browseResult.SelectedFile;
                        }

                        // If read-only set the attribute
                        if (readOnly)
                        {
                            var info = tempFile.TempPath.GetFileInfo();
                            info.Attributes |= FileAttributes.ReadOnly;
                        }

                        // Open the file
                        using (var p = await RCPServices.File.LaunchFileAsync(programPath.Value, arguments: $"\"{tempFile.TempPath}\""))
                        {
                            // Ignore if the file wasn't opened
                            if (p == null)
                            {
                                RL.Logger?.LogTraceSource($"The file was not opened");
                                return;
                            }

                            // Wait for the file to close...
                            await p.WaitForExitAsync();
                        }

                        // If read-only we don't need to check if it has been modified
                        if (readOnly)
                            return;

                        // Open the temp file
                        using FileStream tempFileStream = new FileStream(tempFile.TempPath, FileMode.Open, FileAccess.Read);

                        // Get the new hash
                        var newHash = sha1.ComputeHash(tempFileStream);

                        tempFileStream.Position = 0;

                        // Check if the file has been modified
                        if (!originalHash.SequenceEqual(newHash))
                        {
                            RL.Logger?.LogTraceSource($"The file was modified");

                            // Import the modified file
                            ImportFile(tempFileStream, ext, convert);
                        }
                        else
                        {
                            RL.Logger?.LogTraceSource($"The file was not modified");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Opening archive file for editing");

                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ViewEditFileError);
                    }
                }
            }
        }

        public async Task RenameFileAsync()
        {
            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    var result = await RCPServices.UI.GetStringInput(new StringInputViewModel
                    {
                        Title = Resources.Archive_SetFileName,
                        HeaderText = Resources.Archive_SetFileName,
                        StringInput = FileName
                    });

                    if (result.CanceledByUser)
                        return;

                    var newName = result.StringInput;
                    var dir = ArchiveDirectory;

                    // Check if the name conflicts with an existing file
                    if (ArchiveDirectory.Files.Any(x => x.FileName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        await Services.MessageUI.DisplayMessageAsync(Resources.Archive_FileNameConflict, Resources.Archive_AddFiles_ConflictHeader, MessageType.Error);
                        return;
                    }

                    // Create a new file
                    var newFile = new ArchiveFileViewModel(new ArchiveFileItem(Manager, newName, dir.FullPath, Manager.GetNewFileEntry(Archive.ArchiveData, dir.FullPath, newName)), dir);

                    // Set the file type
                    newFile.SetFileType(FileType);

                    // Copy the file contents
                    newFile.ReplaceFile(GetDecodedFileStream().Stream);

                    // Add the new file
                    dir.Files.Insert(dir.Files.IndexOf(this), newFile);

                    // Delete this file
                    DeleteFile();

                    RL.Logger?.LogTraceSource($"The file {FileName} was renamed to {newName}");
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
            FileData?.Dispose();
        }

        #endregion

        #region Classes

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
}