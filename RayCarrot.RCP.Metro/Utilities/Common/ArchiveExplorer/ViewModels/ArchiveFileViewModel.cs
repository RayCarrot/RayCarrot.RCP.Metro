using System;
using System.IO;
using System.Windows.Media;
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
        /// <param name="archiveFileStream">The archive file stream</param>
        public ArchiveFileViewModel(IArchiveFileData fileData, FileStream archiveFileStream)
        {
            FileData = fileData;
            ArchiveFileStream = archiveFileStream;
            FileName = FileData.FileName;
        }

        #endregion

        #region Public Properties

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
        protected FileStream ArchiveFileStream { get; }

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
                // Get the thumbnail
                var img = imgData.
                    // Get the bitmap image
                    GetBitmap(ArchiveFileStream, 64, 64).
                    // Get an image source from the bitmap
                    ToImageSource();

                // Freeze the image to avoid thread errors
                img.Freeze();

                // Set the image source
                ThumbnailSource = img;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}