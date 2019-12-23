using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a file in an archive
    /// </summary>
    public class ArchiveFileViewModel : BaseRCPViewModel
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
            ExportCommand = new AsyncRelayCommand(ExportFileAsync);
            ImportCommand = new AsyncRelayCommand(ImportFileAsync);
        }

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

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
        /// The archive file stream
        /// </summary>
        public FileStream ArchiveFileStream => Archive.ArchiveFileStream;

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the thumbnail image source for the file
        /// </summary>
        public void LoadThumbnail()
        {
            // Get the bitmap if the item is an image
            if (FileData is IArchiveImageFileData imgData)
            {
                try
                {
                    // Get the thumbnail
                    var img = imgData.
                        // Get the bitmap image
                        GetBitmap(ArchiveFileStream, 64).
                        // Get an image source from the bitmap
                        ToImageSource();
                    
                    // Freeze the image to avoid thread errors
                    img.Freeze();

                    // Set the image source
                    ThumbnailSource = img;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting archive file thumbnail");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Exports the file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportFileAsync()
        {
            // Get the output path
            var result = await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                // TODO: Localize
                Title = "Export file to...",
                DefaultName = FileName,
                Extensions = FileData.AvailableFileFormats.ToString()
            });

            if (result.CanceledByUser)
                return;

            // TODO: Try/catch
            // Save the file
            await FileData.SaveFileAsync(ArchiveFileStream, result.SelectedFileLocation, result.SelectedFileLocation.FileExtension);

            // TODO: Success message
        }

        /// <summary>
        /// Imports a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportFileAsync()
        {
            // TODO: Try/catch

            // Get the file
            var result = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                // TODO: Localize
                Title = "Select file to import",
                ExtensionFilter = FileData.AvailableFileFormats.ToString()
            });

            if (result.CanceledByUser)
                return;

            // Import the file
            await FileData.ImportFileAsync(ArchiveFileStream, result.SelectedFile);

            // Update the archive
            await Archive.UpdateArchiveAsync();
        }

        #endregion
    }
}