using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a file in an archive
    /// </summary>
    public class ArchiveFileViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="archive">The archive the file belongs to</param>
        public ArchiveFileViewModel(IArchiveFileData fileData, ArchiveViewModel archive)
        {
            // Set properties
            FileData = fileData;
            Archive = archive;
            FileName = FileData.FileName;

            // Create commands
            ExportCommand = new AsyncRelayCommand(async () => await ExportFileAsync(false));
            ExportMipmapsCommand = new AsyncRelayCommand(async () => await ExportFileAsync(true));
            ImportCommand = new AsyncRelayCommand(ImportFileAsync);
        }

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

        public ICommand ExportMipmapsCommand { get; }

        public ICommand ImportCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The archive the file belongs to
        /// </summary>
        public ArchiveViewModel Archive { get; }

        /// <summary>
        /// The file data
        /// </summary>
        public IArchiveFileData FileData { get; }

        /// <summary>
        /// The image source for the thumbnail
        /// </summary>
        public ImageSource ThumbnailSource { get; set; }

        /// <summary>
        /// Indicates if the thumbnail has been loaded and data has been initialized
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The archive file stream
        /// </summary>
        public FileStream ArchiveFileStream => Archive.ArchiveFileStream;

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The native file extension
        /// </summary>
        public FileExtension NativeFileExtension => new FileExtension(FileName);

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public string FileDisplayInfo => FileData.FileDisplayInfo;

        /// <summary>
        /// Indicates if the file is an image with mipmaps
        /// </summary>
        public bool HasMipmaps { get; set; }

        /// <summary>
        /// Indicates if the file is an image
        /// </summary>
        public bool IsImage => FileData is IArchiveImageFileData;

        /// <summary>
        /// The icon kind to use for the file
        /// </summary>
        public PackIconMaterialKind IconKind => FileData.IconKind;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the collection of file filter items from a collection of file extensions
        /// </summary>
        /// <param name="fileExtensions">The file extensions</param>
        /// <returns>The file filter item collection</returns>
        protected FileFilterItemCollection GetFileFilterCollection(FileExtension[] fileExtensions)
        {
            return new FileFilterItemCollection(fileExtensions.Select(x => x.GetFileFilterItem));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the thumbnail image source for the file
        /// </summary>
        public void LoadThumbnail()
        {
            // Default to not having mipmaps
            HasMipmaps = false;

            // Get the file bytes
            var bytes = FileData.GetDecodedFileBytes(ArchiveFileStream, Archive.ArchiveFileGenerator, false);

            // Get the bitmap if the item is an image
            if (FileData is IArchiveImageFileData imgData)
            {
                try
                {
                    // Get the thumbnail
                    var img = imgData.GetThumbnail(bytes, 64);

                    // Freeze the image to avoid thread errors
                    img?.Freeze();

                    // Set the image source
                    ThumbnailSource = img;

                    // Get if the image has mipmaps
                    HasMipmaps = imgData.HasMipmaps;
                }
                catch (Exception ex)
                {
                    // If the stream has closed it's not an error
                    if (ArchiveFileStream.CanRead)
                        ex.HandleExpected("Getting archive file thumbnail");
                    else
                        ex.HandleError("Getting archive file thumbnail");
                }
            }
            else
            {
                RCFCore.Logger?.LogDebugSource("A thumbnail can currently not be generated for non-image files in archives");
            }
        }

        /// <summary>
        /// Exports the file
        /// </summary>
        /// <param name="includeMipmap">Indicates if available mipmaps should be included</param>
        /// <returns>The task</returns>
        public async Task ExportFileAsync(bool includeMipmap)
        {
            RCFCore.Logger?.LogTraceSource($"The archive file {FileName} is being exported...");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // Get the file extensions
                        var ext = (includeMipmap ? FileData.CastTo<IArchiveImageFileData>().SupportedMipmapExportFileExtensions : FileData.SupportedExportFileExtensions);

                        // Get the output path
                        var result = await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
                        {
                            Title = Resources.Archive_ExportHeader,
                            DefaultName = new FileSystemPath(FileName).ChangeFileExtension(ext.First().GetPrimaryFileExtension(), true),
                            Extensions = GetFileFilterCollection(ext).ToString()
                        });

                        if (result.CanceledByUser)
                            return;

                        Archive.SetDisplayStatus(String.Format(Resources.Archive_ExportingFileStatus, FileName));

                        try
                        {
                            // Get the file bytes
                            var bytes = FileData.GetDecodedFileBytes(ArchiveFileStream, Archive.ArchiveFileGenerator, false);

                            // Get the file format
                            var format = result.SelectedFileLocation.FileExtension;

                            if (!includeMipmap)
                            {
                                // Export the file
                                ExportFile(result.SelectedFileLocation, bytes, format);
                            }
                            else
                            {
                                // Helper method for getting the mipmap file stream
                                Stream GetMipmapStream(int index)
                                {
                                    var path = $"{result.SelectedFileLocation.RemoveFileExtension().FullPath} ({index}){format.FileExtensions}";

                                    return File.Open(path, FileMode.Create, FileAccess.Write);
                                }

                                // Export the mipmaps
                                FileData.CastTo<IArchiveImageFileData>().ExportMipmaps(bytes, GetMipmapStream, format);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Exporting archive file", FileName);

                            await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, FileName));

                            return;
                        }
                        finally
                        {
                            Archive.SetDisplayStatus(String.Empty);
                        }

                        RCFCore.Logger?.LogTraceSource($"The archive file has been exported");

                        await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ExportFileSuccess);
                    });
                }
            }
        }

        /// <summary>
        /// Exports the specified file
        /// </summary>
        /// <param name="filePath">The file path to export to</param>
        /// <param name="fileBytes">The file bytes for the file to export</param>
        /// <param name="format">The format to export as</param>
        /// <returns>The task</returns>
        public void ExportFile(FileSystemPath filePath, byte[] fileBytes, FileExtension format)
        {
            RCFCore.Logger?.LogTraceSource($"An archive file is being exported as {format}");

            // Create the file and open it
            using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);

            // Write the bytes directly to the stream if the specified format is the native one
            if (format == NativeFileExtension)
                fileStream.Write(fileBytes);
            // Convert the file if the format is not the native one
            else
                FileData.ExportFile(fileBytes, fileStream, format);
        }

        /// <summary>
        /// Imports a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportFileAsync()
        {
            RCFCore.Logger?.LogTraceSource($"The archive file {FileName} is being imported...");

            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await Archive.ArchiveLock.LockAsync())
                {
                    // Run as a task
                    await Task.Run(async () =>
                    {
                        // Get the file
                        var result = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                        {
                            Title = Resources.Archive_ImportFileHeader,
                            ExtensionFilter = GetFileFilterCollection(FileData.SupportedExportFileExtensions).CombineAll(Resources.Archive_FileSelectionGroupName).ToString()
                        });

                        if (result.CanceledByUser)
                            return;

                        // Import the file
                        ArchiveImportData importData = ImportFile(result.SelectedFile);

                        // Update the archive
                        var repackSucceeded = await Archive.UpdateArchiveAsync(new ArchiveImportData[]
                        {
                            importData
                        });

                        if (!repackSucceeded)
                            return;

                        RCFCore.Logger?.LogTraceSource($"The archive file has been imported");

                        await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ImportFileSuccess);
                    });
                }
            }
        }

        /// <summary>
        /// Imports the specified file
        /// </summary>
        /// <param name="filePath">The file path to import from</param>
        /// <returns>The import data</returns>
        public ArchiveImportData ImportFile(FileSystemPath filePath)
        {
            // Return the import data
            return new ArchiveImportData(FileData.FileEntryData, file =>
            {
                Archive.SetDisplayStatus(String.Format(Resources.Archive_ImportingFileStatus, FileName));

                RCFCore.Logger?.LogTraceSource($"An archive file is being imported as {filePath.FileExtension}");

                // Get the file format
                var format = filePath.FileExtension;

                byte[] importBytes;

                // Convert the file if the format is not the native one
                if (format != NativeFileExtension)
                {
                    // Get the file bytes
                    var bytes = FileData.GetDecodedFileBytes(ArchiveFileStream, Archive.ArchiveFileGenerator, false);

                    // Open the file to import from
                    using var importFileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);

                    // Create a memory stream for the bytes to encode
                    using var importStream = new MemoryStream();

                    // Convert the file
                    FileData.ConvertImportData(bytes, importFileStream, importStream, format);

                    importBytes = importStream.ToArray();
                }
                else
                {
                    // Read the file bytes to import from
                    importBytes = File.ReadAllBytes(filePath);
                }

                // Return the encoded file
                return Archive.Manager.EncodeFile(importBytes, file);
            });
        }

        #endregion
    }
}