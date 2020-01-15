using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a file in an archive
    /// </summary>
    public class ArchiveFileViewModel : BaseViewModel, IDisposable
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
        public PackIconMaterialKind IconKind => IsImage ? PackIconMaterialKind.FileImageOutline : PackIconMaterialKind.FileOutline;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the collection of file filter items from a collection of file extensions
        /// </summary>
        /// <param name="fileExtensions">The file extensions</param>
        /// <returns>The file filter item collection</returns>
        protected FileFilterItemCollection GetFileFilterCollection(string[] fileExtensions)
        {
            return new FileFilterItemCollection(fileExtensions.
                // Create a filter item for each extension
                Select(x => new FileFilterItem(
                    // Set the filter to the extension
                    $"*{x}", 
                    // Set the name to the extension, without the period, and in upper case
                    x.Substring(1).ToUpper())));
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

            // Get the bitmap if the item is an image
            if (FileData is IArchiveImageFileData imgData)
            {
                try
                {
                    // Get the thumbnail
                    var img = imgData.
                        // Get the bitmap image
                        GetThumbnail(ArchiveFileStream, 64)?.
                        // Get an image source from the bitmap
                        ToImageSource();

                    // Freeze the image to avoid thread errors
                    img?.Freeze();

                    // Set the image source
                    ThumbnailSource = img;

                    // Initialize the data
                    FileData.InitializeData(ArchiveFileStream);

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
                // Initialize the data
                FileData.InitializeData(ArchiveFileStream);

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
                            DefaultName = new FileSystemPath(FileName).ChangeFileExtension(ext.First(), true),
                            Extensions = GetFileFilterCollection(ext).ToString()
                        });

                        if (result.CanceledByUser)
                            return;

                        Archive.SetDisplayStatus(String.Format(Resources.Archive_ExportingFileStatus, FileName));

                        try
                        {
                            if (!includeMipmap)
                                // Export the file
                                await FileData.ExportFileAsync(ArchiveFileStream, result.SelectedFileLocation, result.SelectedFileLocation.FileExtension);
                            else
                                // Export the mipmaps
                                await ((IArchiveImageFileData)FileData).ExportMipmapsAsync(ArchiveFileStream, result.SelectedFileLocation, result.SelectedFileLocation.FileExtension);
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

                        await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ExportFileSuccess);
                    });
                }
            }
        }

        /// <summary>
        /// Imports a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportFileAsync()
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
                        // Get the file
                        var result = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                        {
                            Title = Resources.Archive_ImportFileHeader,
                            ExtensionFilter = GetFileFilterCollection(FileData.SupportedExportFileExtensions).CombineAll(Resources.Archive_FileSelectionGroupName).ToString()
                        });

                        if (result.CanceledByUser)
                            return;

                        Archive.SetDisplayStatus(String.Format(Resources.Archive_ImportingFileStatus, FileName));

                        try
                        {
                            // Import the file
                            var succeeded = await FileData.ImportFileAsync(ArchiveFileStream, result.SelectedFile);

                            // Make sure it succeeded
                            if (!succeeded)
                                return;
                        }
                        catch (Exception ex)
                        {
                            ex.HandleError("Importing archive file", FileName);

                            await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ImportError, result.SelectedFile.Name));

                            return;
                        }
                        finally
                        {
                            Archive.SetDisplayStatus(String.Empty);
                        }

                        // Update the archive
                        var repackSucceeded = await Archive.UpdateArchiveAsync();

                        if (!repackSucceeded)
                            return;

                        await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ImportFileSuccess);
                    });
                }
            }
        }

        public void Dispose()
        {
            // Delete temp file
            RCFRCP.File.DeleteFile(FileData.PendingImportTempPath);
        }

        #endregion
    }
}