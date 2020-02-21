using RayCarrot.IO;
using System;
using System.IO;
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

        /// <summary>
        /// Exports the mipmaps from the file
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="outputStreams">The function used to get the output streams for the mipmaps</param>
        /// <param name="format">The file extension to use</param>
        void ExportMipmaps(byte[] fileBytes, Func<int, Stream> outputStreams, FileExtension format);

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