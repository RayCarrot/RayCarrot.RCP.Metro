using MahApps.Metro.IconPacks;
using RayCarrot.Binary;
using RayCarrot.IO;
using System.IO;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles data for a specific file type
    /// </summary>
    public interface IArchiveFileType
    {
        // TODO-UPDATE: Set this when creating the class!
        /// <summary>
        /// The serializer settings for the archive
        /// </summary>
        BinarySerializerSettings SerializerSettings { set; }

        /// <summary>
        /// The display name for the file type
        /// </summary>
        string TypeDisplayName { get; }

        /// <summary>
        /// The default icon kind for the type
        /// </summary>
        PackIconMaterialKind Icon { get; }

        /// <summary>
        /// Indicates if a file with the specifies file extension is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        bool IsOfType(FileExtension fileExtension);

        /// <summary>
        /// Indicates if a file with the specifies file extension and data is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <param name="inputStream">The file data to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        bool IsOfType(FileExtension fileExtension, Stream inputStream);

        /// <summary>
        /// The native file format
        /// </summary>
        FileExtension NativeFormat { get; }

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        FileExtension[] ImportFormats { get; }

        /// <summary>
        /// The supported formats to export to
        /// </summary>
        FileExtension[] ExportFormats { get; }

        /// <summary>
        /// Gets the thumbnail for the file if it has image data
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="width">The thumbnail width</param>
        /// <returns>The thumbnail, or null if not available</returns>
        ImageSource GetThumbnail(Stream inputStream, int width);

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="format">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        void ConvertTo(FileExtension format, Stream inputStream, Stream outputStream);

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="format">The format to convert from</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        void ConvertFrom(FileExtension format, Stream inputStream, Stream outputStream);
    }
}