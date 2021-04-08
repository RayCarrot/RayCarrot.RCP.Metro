using MahApps.Metro.IconPacks;
using RayCarrot.IO;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles data for a specific file type
    /// </summary>
    public interface IArchiveFileType
    {
        /// <summary>
        /// The display name for the file type
        /// </summary>
        string TypeDisplayName { get; }

        /// <summary>
        /// The default icon kind for the type
        /// </summary>
        PackIconMaterialKind Icon { get; }

        /// <summary>
        /// Indicates if the specified manager supports files of this type
        /// </summary>
        /// <param name="manager">The manager to check</param>
        /// <returns>True if supported, otherwise false</returns>
        bool IsSupported(IArchiveDataManager manager);

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
        /// <param name="manager">The manager</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        bool IsOfType(FileExtension fileExtension, Stream inputStream, IArchiveDataManager manager);

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        FileExtension[] ImportFormats { get; }

        /// <summary>
        /// The supported formats to export to
        /// </summary>
        FileExtension[] ExportFormats { get; }

        /// <summary>
        /// Initializes the file
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="fileExtension">The file extension</param>
        /// <param name="width">The thumbnail width</param>
        /// <param name="manager">The manager</param>
        /// <returns>The init data</returns>
        ArchiveFileInitData InitFile(ArchiveFileStream inputStream, FileExtension fileExtension, int? width, IArchiveDataManager manager);

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="inputFormat">The format to convert from</param>
        /// <param name="outputFormat">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, Stream inputStream, Stream outputStream, IArchiveDataManager manager);

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="inputFormat">The format to convert from</param>
        /// <param name="outputFormat">The format to convert to</param>
        /// <param name="currentFileStream">The current file stream</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream, Stream inputStream, Stream outputStream, IArchiveDataManager manager);
    }
}