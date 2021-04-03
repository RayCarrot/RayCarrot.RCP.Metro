using MahApps.Metro.IconPacks;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    [DebuggerDisplay("{FileName}")]
    public class ArchiveFileViewModel : BaseViewModel, IDisposable
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
            FileExports = new ObservableCollection<ArchiveFileExportViewModel>();

            // Create commands
            ImportCommand = new AsyncRelayCommand(ImportFileAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteFileAsync);

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(FileExports, Application.Current);
            BindingOperations.EnableCollectionSynchronization(FileDisplayInfo, Application.Current);
        }

        #endregion

        #region Commands

        public ICommand ImportCommand { get; }

        public ICommand DeleteCommand { get; }

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
        /// The info about the file to display
        /// </summary>
        public ObservableCollection<DuoGridItemViewModel> FileDisplayInfo { get; }

        /// <summary>
        /// The icon kind to use for the file
        /// </summary>
        public PackIconMaterialKind IconKind { get; set; }

        /// <summary>
        /// Indicates if the file is initialized
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The file export options for the file
        /// </summary>
        public ObservableCollection<ArchiveFileExportViewModel> FileExports { get;}

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

        /// <summary>
        /// Initializes the file
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

                // TODO-UPDATE: Sometimes a threading error is thrown here - why? Happens when exporting a folder.
                // Populate info
                FileDisplayInfo.Clear();

                // TODO-UPDATE: Localize
                FileDisplayInfo.Add(new DuoGridItemViewModel("Directory:", FileData.Directory));

                FileDisplayInfo.AddRange(Manager.GetFileInfo(Archive.ArchiveData, FileData.ArchiveEntry));

                // Get the type
                FileType = FileData.GetFileType(fileStream);

                // Remove previous export formats
                FileExports.Clear();

                // Add native export format
                // TODO-UPDATE: Localize
                FileExports.Add(new ArchiveFileExportViewModel($"Original ({FileExtension})", new AsyncRelayCommand(async () => await ExportFileAsync())));

                // Get export formats
                FileExports.AddRange(FileType.ExportFormats.Select(x => new ArchiveFileExportViewModel(x.DisplayName, new AsyncRelayCommand(async () => await ExportFileAsync(x)))));

                fileStream.SeekToBeginning();

                // Initialize the file
                var initData = FileType.InitFile(fileStream, FileExtension, loadThumbnail ? (int?)64 : null, Manager);

                if (loadThumbnail)
                {
                    // Get the thumbnail
                    var img = initData.Thumbnail;

                    // Freeze the image to avoid thread errors
                    img?.Freeze();

                    // Set the image source
                    ThumbnailSource = img;
                }

                // Set icon
                IconKind = FileType.Icon;

                // Add display info from the init data
                FileDisplayInfo.AddRange(initData.FileInfo);
                FileDisplayInfo.Add(new DuoGridItemViewModel("Type:", FileType.TypeDisplayName, UserLevel.Advanced));

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                // If the stream has closed it's not an error
                if (ArchiveFileStream.CanRead)
                    ex.HandleExpected("Initializing file");
                else
                    ex.HandleError("Initializing file");

                IconKind = PackIconMaterialKind.FileAlertOutline;
            }
            finally
            {
                if (!hadStream)
                    fileStream?.Dispose();
            }
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
                        ExtensionFilter = new FileFilterItemCollection(FileType.ExportFormats.Select(x => x.GetFileFilterItem)).CombineAll(Resources.Archive_FileSelectionGroupName).ToString()
                    });

                    if (result.CanceledByUser)
                        return;

                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // TODO-UPDATE: Try/catch all of this

                        // Open the file to be imported
                        using (var importFile = File.OpenRead(result.SelectedFile))
                        {
                            // Memory stream for converted data
                            using var memStream = new MemoryStream();

                            // If it's being imported from a non-native format, convert it
                            var convert = result.SelectedFile.FileExtension != FileExtension;

                            if (convert)
                                // Convert from the imported file to the memory stream
                                FileType.ConvertFrom(result.SelectedFile.FileExtension, FileExtension, GetDecodedFileStream(), importFile, memStream, Manager);

                            // Replace the file with the import data
                            if (ReplaceFile(convert ? (Stream)memStream : importFile))
                                Archive.AddModifiedFiles();
                        }

                        RL.Logger?.LogTraceSource($"The archive file is pending to be imported");
                    });
                }
            }
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

            // If no data was encoded we copy over the decoded data
            if (FileData.PendingImport.Length == 0)
                inputStream.CopyTo(FileData.PendingImport);

            HasPendingImport = true;

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
                    // Remove the file from the directory
                    ArchiveDirectory.Files.Remove(this);

                    // Dispose the file
                    Dispose();

                    // Add as modified file
                    Archive.AddModifiedFiles();

                    RL.Logger?.LogTraceSource($"The archive file has been removed");
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
            BindingOperations.DisableCollectionSynchronization(FileDisplayInfo);

            // Dispose the file data
            FileData?.Dispose();
        }

        #endregion

        #region Classes

        /// <summary>
        /// View model for an archive file export option
        /// </summary>
        public class ArchiveFileExportViewModel : BaseViewModel
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="displayName">The format display name</param>
            /// <param name="exportCommand">The export command</param>
            public ArchiveFileExportViewModel(string displayName, ICommand exportCommand)
            {
                DisplayName = displayName;
                ExportCommand = exportCommand;
            }

            /// <summary>
            /// The format display name
            /// </summary>
            public string DisplayName { get; }

            /// <summary>
            /// The export command
            /// </summary>
            public ICommand ExportCommand { get; }
        }

        #endregion
    }
}