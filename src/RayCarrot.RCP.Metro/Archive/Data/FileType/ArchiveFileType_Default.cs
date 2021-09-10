using MahApps.Metro.IconPacks;
using RayCarrot.IO;
using System;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The default file type
    /// </summary>
    public class ArchiveFileType_Default : IArchiveFileType
    {
        #region Interface Implementations

        /// <summary>
        /// The display name for the file type
        /// </summary>
        public string TypeDisplayName => Resources.Archive_Format_Default;

        /// <summary>
        /// The default icon kind for the type
        /// </summary>
        public PackIconMaterialKind Icon => PackIconMaterialKind.FileOutline;

        /// <summary>
        /// Indicates if the specified manager supports files of this type
        /// </summary>
        /// <param name="manager">The manager to check</param>
        /// <returns>True if supported, otherwise false</returns>
        public bool IsSupported(IArchiveDataManager manager) => true;

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
        /// <param name="manager">The manager</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public virtual bool IsOfType(FileExtension fileExtension, Stream inputStream, IArchiveDataManager manager) => true;

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        public FileExtension[] ImportFormats => new FileExtension[0];

        /// <summary>
        /// The supported formats to export to
        /// </summary>
        public FileExtension[] ExportFormats => new FileExtension[0];

        /// <summary>
        /// Loads the thumbnail and display info for the file
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="fileExtension">The file extension</param>
        /// <param name="width">The thumbnail width</param>
        /// <param name="manager">The manager</param>
        /// <returns>The thumbnail data</returns>
        public ArchiveFileThumbnailData LoadThumbnail(ArchiveFileStream inputStream, FileExtension fileExtension, int width, IArchiveDataManager manager) => new(null, new DuoGridItemViewModel[0]);

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="inputFormat">The format to convert from</param>
        /// <param name="outputFormat">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        public virtual void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, Stream inputStream, Stream outputStream, IArchiveDataManager manager) => throw new NotSupportedException("A default file types can't be converted");

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="inputFormat">The format to convert from</param>
        /// <param name="outputFormat">The format to convert to</param>
        /// <param name="currentFileStream">The current file stream</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        public virtual void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream, Stream inputStream, Stream outputStream, IArchiveDataManager manager) => throw new NotSupportedException("A default file types can't be converted");

        #endregion
    }
}