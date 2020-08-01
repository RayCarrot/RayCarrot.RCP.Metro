using System;
using System.IO;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using RayCarrot.Binary;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The default file type
    /// </summary>
    public class ArchiveFileType_Default : IArchiveFileType
    {
        #region Interface Implementations

        /// <summary>
        /// The serializer settings for the archive
        /// </summary>
        public BinarySerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// The display name for the file type
        /// </summary>
        public string TypeDisplayName => "File";

        /// <summary>
        /// The default icon kind for the type
        /// </summary>
        public PackIconMaterialKind Icon => PackIconMaterialKind.FileOutline;

        /// <summary>
        /// Indicates if a file with the specifies file extension is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public virtual bool IsOfType(FileExtension fileExtension) => true;

        /// <summary>
        /// Indicates if a file with the specifies file extension and data is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <param name="inputStream">The file data to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public virtual bool IsOfType(FileExtension fileExtension, Stream inputStream) => true;

        /// <summary>
        /// The native file format
        /// </summary>
        public FileExtension NativeFormat => new FileExtension(String.Empty);

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        public FileExtension[] ImportFormats => new FileExtension[0];

        /// <summary>
        /// The supported formats to export to
        /// </summary>
        public FileExtension[] ExportFormats => new FileExtension[0];

        /// <summary>
        /// Gets the thumbnail for the file if it has image data
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="width">The thumbnail width</param>
        /// <returns>The thumbnail, or null if not available</returns>
        public ImageSource GetThumbnail(Stream inputStream, int width) => null;

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="format">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        public virtual void ConvertTo(FileExtension format, Stream inputStream, Stream outputStream) => throw new NotSupportedException("A default file types can't be converted");

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="format">The format to convert from</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        public virtual void ConvertFrom(FileExtension format, Stream inputStream, Stream outputStream) => throw new NotSupportedException("A default file types can't be converted");

        #endregion
    }
}