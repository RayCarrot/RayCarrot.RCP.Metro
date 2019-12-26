using System.Drawing;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive file data for an image
    /// </summary>
    public interface IArchiveImageFileData : IArchiveFileData
    {
        /// <summary>
        /// Gets the image as a bitmap
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The image as a bitmap</returns>
        Bitmap GetBitmap(Stream archiveFileStream);

        /// <summary>
        /// Gets the image as a bitmap with a specified width, while maintaining the aspect ratio
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="width">The width</param>
        /// <returns>The image as a bitmap</returns>
        Bitmap GetBitmap(Stream archiveFileStream, int width);
    }
}