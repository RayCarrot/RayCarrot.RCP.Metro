using ByteSizeLib;
using MahApps.Metro.IconPacks;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archived file data for a UbiArt .ipk file
    /// </summary>
    public class UbiArtIPKArchiveFileData : IArchiveFileData
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileEntry">The file data</param>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="baseOffset">The base offset to use when reading the files</param>
        public UbiArtIPKArchiveFileData(UbiArtIPKFileEntry fileEntry, UbiArtSettings settings, uint baseOffset)
        {
            // Get the file properties
            Directory = fileEntry.Path.DirectoryPath;
            FileExtension = new FileExtension(fileEntry.Path.GetFileExtensions());
            FileName = fileEntry.Path.FileName;
            FileEntry = fileEntry;
            BaseOffset = baseOffset;
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected UbiArtSettings Settings { get; }

        /// <summary>
        /// The base offset to use when reading the files
        /// </summary>
        protected uint BaseOffset { get; }

        /// <summary>
        /// Indicates if the data has been initialized
        /// </summary>
        protected bool HasInitializedData { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The directory the file is located under
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// The file data
        /// </summary>
        public UbiArtIPKFileEntry FileEntry { get; }

        /// <summary>
        /// The file entry data
        /// </summary>
        public object FileEntryData => FileEntry;

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public virtual string FileDisplayInfo => String.Format(
            Resources.Archive_IPK_FileInfo,
            Directory,
            FileEntry.IsCompressed,
            ByteSize.FromBytes(FileEntry.Size),
            ByteSize.FromBytes(FileEntry.CompressedSize),
            FileEntry.Offsets.First() + BaseOffset);

        /// <summary>
        /// The default icon to use for this file
        /// </summary>
        public virtual PackIconMaterialKind IconKind => PackIconMaterialKind.FileOutline;

        /// <summary>
        /// The name of the file format
        /// </summary>
        public string FileFormatName => FileExtension.DisplayName;

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public virtual FileExtension[] SupportedImportFileExtensions
        {
            get => new FileExtension[]
            {
                FileExtension
            };
            set => throw new InvalidOperationException();
        }

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public virtual FileExtension[] SupportedExportFileExtensions
        {
            get => new FileExtension[]
            {
                FileExtension
            };
            set => throw new InvalidOperationException();
        }

        /// <summary>
        /// The file extension
        /// </summary>
        public FileExtension FileExtension { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the decoded contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="generator">The file generator</param>
        /// <param name="initilizeOnly">Indicates if the bytes should be retrieved for initialization only, in which case the bytes don't need to be returned</param>
        /// <returns>The contents of the file</returns>
        public byte[] GetDecodedFileBytes(Stream archiveFileStream, IDisposable generator, bool initilizeOnly)
        {
            // Don't read the bytes if it's only for initialization and the file has been initialized
            if (initilizeOnly && HasInitializedData)
                return null;

            // Get the bytes
            var bytes = GetEncodedFileBytes(archiveFileStream, generator);

            // Decompress the data if compressed
            if (FileEntry.IsCompressed)
                bytes = UbiArtIpkData.GetEncoder(Settings.IPKVersion, FileEntry.Size).Decode(bytes);

            // Initialize the data
            InitializeData(bytes);

            // Return the bytes
            return bytes;
        }

        /// <summary>
        /// Gets the original encoded contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="generator">The file generator</param>
        /// <returns>The contents of the file</returns>
        public byte[] GetEncodedFileBytes(Stream archiveFileStream, IDisposable generator)
        {
            // Get the bytes
            return generator.CastTo<IArchiveFileGenerator<UbiArtIPKFileEntry>>().GetBytes(FileEntry);
        }

        /// <summary>
        /// Initializes the data for the file
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        public virtual void InitializeData(byte[] fileBytes) { }

        /// <summary>
        /// Exports the file to the stream in the specified format
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="outputStream">The stream to export to</param>
        /// <param name="format">The file format to use</param>
        public virtual void ExportFile(byte[] fileBytes, Stream outputStream, FileExtension format)
        {
            // Write to the stream
            outputStream.Write(fileBytes);
        }

        /// <summary>
        /// Converts the import file data from the input stream to the output stream
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="inputStream">The input stream to import from</param>
        /// <param name="outputStream">The destination stream</param>
        /// <param name="format">The file format to use</param>
        public virtual void ConvertImportData(byte[] fileBytes, Stream inputStream, Stream outputStream, FileExtension format)
        {
            // Copy the file
            inputStream.CopyTo(outputStream);
        }

        #endregion
    }
}