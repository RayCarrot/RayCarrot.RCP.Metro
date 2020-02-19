using ByteSizeLib;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archived file data for an OpenSpace .cnt file
    /// </summary>
    public class OpenSpaceCntArchiveFileData : IArchiveImageFileData
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileEntry">The file data</param>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="directory">The directory the file is located under</param>
        /// <param name="encryptFiles">Indicates if the files should be encrypted when imported</param>
        public OpenSpaceCntArchiveFileData(OpenSpaceCntFileEntry fileEntry, OpenSpaceSettings settings, string directory, bool encryptFiles)
        {
            Directory = directory;
            EncryptFiles = encryptFiles;
            FileEntry = fileEntry;
            FileName = FileEntry.FileName;
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected OpenSpaceSettings Settings { get; }

        /// <summary>
        /// Indicates if the files should be encrypted when imported
        /// </summary>
        protected bool EncryptFiles { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The directory the file is located under
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// The file data
        /// </summary>
        public OpenSpaceCntFileEntry FileEntry { get; }

        /// <summary>
        /// The file entry data
        /// </summary>
        public object FileEntryData => FileEntry;

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The file size height
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// The file size width
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Indicates if the file uses transparency
        /// </summary>
        public bool IsTransparent { get; set; }

        /// <summary>
        /// The number of available mipmaps for the image
        /// </summary>
        public int Mipmaps { get; set; }

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public string FileDisplayInfo => String.Format(
            Resources.Archive_CNT_FileInfo, 
            Directory, 
            Width, Height, 
            ByteSize.FromBytes(FileEntry.Size), 
            FileEntry.Unknown1 == 0,
            FileEntry.FileXORKey.Any(x => x != 0), 
            IsTransparent, 
            FileEntry.Pointer, 
            Mipmaps);

        /// <summary>
        /// The default icon to use for this file
        /// </summary>
        public PackIconMaterialKind IconKind => PackIconMaterialKind.FileImageOutline;

        /// <summary>
        /// The name of the file format
        /// </summary>
        public string FileFormatName => ".GF";

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public FileExtension[] SupportedImportFileExtensions => ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileExtension(x)).Append(new FileExtension(".gf")).ToArray();

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public FileExtension[] SupportedExportFileExtensions => ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileExtension(x)).Append(new FileExtension(".gf")).ToArray();

        /// <summary>
        /// The supported file formats for exporting mipmaps
        /// </summary>
        public FileExtension[] SupportedMipmapExportFileExtensions => ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileExtension(x)).ToArray();

        /// <summary>
        /// Indicates if the image has mipmaps
        /// </summary>
        public bool HasMipmaps => Mipmaps > 0;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="generator">The file generator</param>
        /// <returns>The contents of the file</returns>
        public byte[] GetFileBytes(Stream archiveFileStream, IDisposable generator)
        {
            // Get the bytes
            var bytes = generator.CastTo<IArchiveFileGenerator<OpenSpaceCntFileEntry>>().GetBytes(FileEntry);

            // Decrypt the bytes
            OpenSpaceCntData.DecryptFileData(bytes, FileEntry.FileXORKey);

            return bytes;
        }

        /// <summary>
        /// Gets the contents of the file with an option to deserialize mipmaps
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="deserializeMipmap">Indicates if mipmaps should be deserialized if available</param>
        /// <returns>The deserialized file</returns>
        public OpenSpaceGFFile GetFileContent(byte[] fileBytes, bool deserializeMipmap)
        {
            // Set if mipmaps should be deserialized
            Settings.DeserializeMipmaps = deserializeMipmap;

            // Load the bytes into a memory stream
            using var stream = new MemoryStream(fileBytes);

            // Serialize the data
            var data = OpenSpaceGFFile.GetSerializer(Settings).Deserialize(stream);

            // Make sure we read the entire file
            if (stream.Position != stream.Length)
                RCFCore.Logger?.LogWarningSource($"The GF file {FileName} was not fully read");

            // Return the data
            return data;
        }

        /// <summary>
        /// Gets the image as a bitmap
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <returns>The image as a bitmap</returns>
        public Bitmap GetBitmap(byte[] fileBytes)
        {
            // Load the file
            var file = GetFileContent(fileBytes, false);

            // Get the bitmap
            return file.GetBitmap();
        }

        /// <summary>
        /// Gets all images, including mipmaps, as bitmaps
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <returns>The images as a bitmaps</returns>
        public IEnumerable<Bitmap> GetBitmaps(byte[] fileBytes)
        {
            // Load the file
            var file = GetFileContent(fileBytes, true);

            // Get the bitmaps
            return file.GetBitmaps(true);
        }

        /// <summary>
        /// Gets the image as an image source with a specified width, while maintaining the aspect ratio
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="width">The width</param>
        /// <returns>The image as an image source</returns>
        public ImageSource GetThumbnail(byte[] fileBytes, int width)
        {
            // Load the file
            var file = GetFileContent(fileBytes, false);

            // Set properties
            Height = file.Height;
            Width = file.Width;
            IsTransparent = file.IsTransparent;
            Mipmaps = file.RealMipmapCount;

            // Get the thumbnail with the specified size
            return file.GetBitmapThumbnail(width)?.ToImageSource();
        }

        /// <summary>
        /// Exports the file to the stream in the specified format
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="outputStream">The stream to export to</param>
        /// <param name="format">The file format to use</param>
        /// <returns>The task</returns>
        public Task ExportFileAsync(byte[] fileBytes, Stream outputStream, FileExtension format)
        {
            // Get the bitmap
            using var bmp = GetBitmap(fileBytes);

            // Get the format
            var imgFormat = ImageHelpers.GetImageFormat(format);

            // Save the file
            bmp.Save(outputStream, imgFormat);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Exports the mipmaps from the file to the specified path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        public Task ExportMipmapsAsync(byte[] fileBytes, FileSystemPath filePath, string fileFormat)
        {
            int index = 0;

            // Save each mipmap
            foreach (var bmp in GetBitmaps(fileBytes))
            {
                // Get the file path
                var mipmapFile = filePath;

                if (index > 0)
                    mipmapFile = mipmapFile.RemoveFileExtension().FullPath + $" ({index}){fileFormat}";

                // Open the file
                using var file = File.Open(mipmapFile, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the format
                var format = ImageHelpers.GetImageFormat(new FileExtension(fileFormat));

                // Save the file
                bmp.Save(file, format);

                index++;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Imports the file from the stream to the output
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="inputStream">The input stream to import from</param>
        /// <param name="outputStream">The destination stream</param>
        /// <param name="format">The file format to use</param>
        /// <returns>The task</returns>
        public Task ImportFileAsync(byte[] fileBytes, Stream inputStream, Stream outputStream, FileExtension format)
        {
            // Load the bitmap
            using var bmp = new Bitmap(inputStream);

            // Load the current file
            var file = GetFileContent(fileBytes, true);

            // Import the bitmap
            file.ImportFromBitmap(bmp);

            // Serialize the data to get the bytes
            OpenSpaceGFFile.GetSerializer(Settings).Serialize(outputStream, file);

            return Task.CompletedTask;
        }

        #endregion
    }
}