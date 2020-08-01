using ImageMagick;
using MahApps.Metro.IconPacks;
using RayCarrot.Binary;
using RayCarrot.IO;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// An image file type
    /// </summary>
    public abstract class ArchiveFileType_Image : IArchiveFileType
    {
        #region Interface Implementations

        /// <summary>
        /// The serializer settings for the archive
        /// </summary>
        public BinarySerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// The display name for the file type
        /// </summary>
        public string TypeDisplayName => Format.ToString().ToUpper();

        /// <summary>
        /// The default icon kind for the type
        /// </summary>
        public PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

        /// <summary>
        /// Indicates if a file with the specifies file extension is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public virtual bool IsOfType(FileExtension fileExtension) => FileExtensions.Contains(fileExtension);

        /// <summary>
        /// Indicates if a file with the specifies file extension and data is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <param name="inputStream">The file data to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public virtual bool IsOfType(FileExtension fileExtension, Stream inputStream) => false;

        /// <summary>
        /// The native file format
        /// </summary>
        public FileExtension NativeFormat => FileExtensions.First();

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        public FileExtension[] ImportFormats => new FileExtension[]
        {
            new FileExtension(".png"),
            new FileExtension(".jpg"),
            new FileExtension(".dds"),
            new FileExtension(".bmp"),
        };

        /// <summary>
        /// The supported formats to export to
        /// </summary>
        public FileExtension[] ExportFormats => ImportFormats;

        /// <summary>
        /// Gets the thumbnail for the file if it has image data
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="width">The thumbnail width</param>
        /// <returns>The thumbnail, or null if not available</returns>
        public ImageSource GetThumbnail(Stream inputStream, int width)
        {
            // Get the image
            using var img = GetImage(inputStream);

            // Resize to a thumbnail
            img.Thumbnail(width, (int)(img.Height / ((double)img.Width / width)));

            // Return as a bitmap source
            return img.ToBitmapSource();
        }

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="format">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        public virtual void ConvertTo(FileExtension format, Stream inputStream, Stream outputStream)
        {
            // Get the image
            using var img = GetImage(inputStream);

            // Write to stream as new format
            img.Write(outputStream, MagickFormatInfo.Create(format.FileExtensions).Format);
        }

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="format">The format to convert from</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        public virtual void ConvertFrom(FileExtension format, Stream inputStream, Stream outputStream)
        {
            // Get the image in specified format
            using var img = new MagickImage(inputStream, MagickFormatInfo.Create(format.FileExtensions).Format);

            // Write to stream as native format
            img.Write(outputStream, Format);
        }

        #endregion

        #region Image Data

        /// <summary>
        /// The image format
        /// </summary>
        public abstract MagickFormat Format { get; }
        
        /// <summary>
        /// The file extensions used by the image format
        /// </summary>
        public abstract FileExtension[] FileExtensions { get; }
        
        /// <summary>
        /// Gets an image from the file data
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <returns>The image</returns>
        protected virtual MagickImage GetImage(Stream inputStream) => new MagickImage(inputStream, Format);

        #endregion
    }
}