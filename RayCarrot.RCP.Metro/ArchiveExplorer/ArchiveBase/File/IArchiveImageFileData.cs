using RayCarrot.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines archive file data for an image
    /// </summary>
    public interface IArchiveImageFileData : IArchiveFileData
    {
        /// <summary>
        /// Gets the image as an image source with a specified width, while maintaining the aspect ratio
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="width">The width</param>
        /// <returns>The image as an image source</returns>
        ImageSource GetThumbnail(byte[] fileBytes, int width);

        // TODO-UPDATE: Update this to new system with FileExtension etc.
        /// <summary>
        /// Exports the mipmaps from the file to the specified path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        Task ExportMipmapsAsync(byte[] fileBytes, FileSystemPath filePath, string fileFormat);

        /// <summary>
        /// Indicates if the image has mipmaps
        /// </summary>
        bool HasMipmaps { get; }

        /// <summary>
        /// The supported file formats for exporting mipmaps
        /// </summary>
        FileExtension[] SupportedMipmapExportFileExtensions { get; }
    }
}