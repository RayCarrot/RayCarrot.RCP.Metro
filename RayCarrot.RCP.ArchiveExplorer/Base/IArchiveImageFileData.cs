using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.ArchiveExplorer
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

        /// <summary>
        /// Exports the mipmaps from the file to the specified path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        Task ExportMipmapsAsync(Stream archiveFileStream, FileSystemPath filePath, string fileFormat);

        /// <summary>
        /// Indicates if the image has mipmaps
        /// </summary>
        bool HasMipmaps { get; }
    }
}