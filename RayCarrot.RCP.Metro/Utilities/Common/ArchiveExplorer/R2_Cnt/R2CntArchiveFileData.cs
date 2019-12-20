using System.Drawing;
using System.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archived file data for a Rayman 2 .cnt file
    /// </summary>
    public class R2CntArchiveFileData : IArchiveImageFileData
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileData">The file data</param>
        public R2CntArchiveFileData(R2CntFile fileData)
        {
            FileData = fileData;
            FileName = FileData.FileName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The file data
        /// </summary>
        public R2CntFile FileData { get; }

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName { get; }

        #endregion

        /// <summary>
        /// Gets the contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The contents of the file</returns>
        public byte[] GetFileContent(Stream archiveFileStream)
        {
            return FileData.GetFileBytes(archiveFileStream);
        }

        /// <summary>
        /// Gets the image as a bitmap
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The image as a bitmap</returns>
        public Bitmap GetBitmap(Stream archiveFileStream)
        {
            return FileData.GetFileContent(archiveFileStream).GetBitmap();
        }

        /// <summary>
        /// Gets the image as a bitmap with a specified size
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>The image as a bitmap</returns>
        public Bitmap GetBitmap(Stream archiveFileStream, int width, int height)
        {
            return FileData.GetFileContent(archiveFileStream).GetBitmapThumbnail(width, height);
        }
    }
}