using ByteSizeLib;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="fileData">The file data</param>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="directory">The directory the file is located under</param>
        /// <param name="encryptFiles">Indicates if the files should be encrypted when imported</param>
        public OpenSpaceCntArchiveFileData(OpenSpaceCntFile fileData, OpenSpaceSettings settings, string directory, bool encryptFiles)
        {
            Directory = directory;
            EncryptFiles = encryptFiles;
            FileData = fileData;
            FileName = FileData.FileName;
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
        public OpenSpaceCntFile FileData { get; }

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
            new ByteSize(FileData.Size), 
            FileData.Unknown1 == 0,
            FileData.FileXORKey.Any(x => x != 0), 
            IsTransparent, 
            FileData.Pointer, 
            Mipmaps);

        /// <summary>
        /// The name of the file format
        /// </summary>
        public string FileFormatName => ".GF";

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public ArchiveFileExtension[] SupportedImportFileExtensions => ImageHelpers.GetSupportedBitmapExtensions().Select(x => new ArchiveFileExtension(x)).Append(new ArchiveFileExtension(".gf")).ToArray();

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public ArchiveFileExtension[] SupportedExportFileExtensions => ImageHelpers.GetSupportedBitmapExtensions().Select(x => new ArchiveFileExtension(x)).Append(new ArchiveFileExtension(".gf")).ToArray();

        /// <summary>
        /// The supported file formats for exporting mipmaps
        /// </summary>
        public ArchiveFileExtension[] SupportedMipmapExportFileExtensions => ImageHelpers.GetSupportedBitmapExtensions().Select(x => new ArchiveFileExtension(x)).ToArray();

        /// <summary>
        /// The path to the temporary file containing the data to be imported
        /// </summary>
        public FileSystemPath PendingImportTempPath { get; set; }

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
        /// <returns>The contents of the file</returns>
        public byte[] GetFileBytes(Stream archiveFileStream)
        {
            return FileData.GetFileBytes(archiveFileStream);
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
            var data = new OpenSpaceGfSerializer(Settings).Deserialize(stream);

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
        /// Exports the file to the specified path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        public Task ExportFileAsync(byte[] fileBytes, FileSystemPath filePath, string fileFormat)
        {
            RCFCore.Logger?.LogInformationSource($"A CNT archive file is being exported as {fileFormat}");

            // Open the file
            using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            // Check if the file should be saved as its native format
            if (fileFormat == ".gf")
            {
                // Write to the stream
                file.Write(fileBytes, 0, fileBytes.Length);
            }
            // Convert the file and save
            else
            {
                // Get the bitmap
                using var bmp = GetBitmap(fileBytes);

                // Get the format
                var format = ImageHelpers.GetImageFormat(fileFormat);

                // Save the file
                bmp.Save(file, format);
            }

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
                var format = ImageHelpers.GetImageFormat(fileFormat);

                // Save the file
                bmp.Save(file, format);

                index++;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Imports the file from the specified path to the <see cref="PendingImportTempPath"/> path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path of the file to import</param>
        /// <returns>A value indicating if the file was successfully imported</returns>
        public Task<bool> ImportFileAsync(byte[] fileBytes, FileSystemPath filePath)
        {
            RCFCore.Logger?.LogInformationSource($"A CNT archive file is being imported as {filePath.FileExtension}");

            // Get the temporary file to save to, without disposing it
            var tempFile = new TempFile(false);

            // Check if the file is in the native format
            if (filePath.FileExtension.Equals(".gf", StringComparison.InvariantCultureIgnoreCase))
            {
                // Copy the file
                RCFRCP.File.CopyFile(filePath, tempFile.TempPath, true);
            }
            // Import as bitmap
            else
            {
                // Load the bitmap
                using var bmp = new Bitmap(filePath);

                // Load the current file
                var file = GetFileContent(fileBytes, true);

                // Import the bitmap
                file.ImportFromBitmap(bmp);

                // Serialize to the file
                using var stream = File.Open(tempFile.TempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                
                // Serialize the data to get the bytes
                new OpenSpaceGfSerializer(Settings).Serialize(stream, file);
            }

            // Encrypt the file if set to do so
            if (EncryptFiles)
            {
                // Open the file with read/write access
                using var fileSteam = File.Open(tempFile.TempPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                // Encrypt each byte
                for (int i = 0; i < fileSteam.Length; i++)
                {
                    if ((fileSteam.Length % 4) + i >= fileSteam.Length) 
                        continue;

                    // Read the byte
                    var b = fileSteam.ReadByte();

                    // Go back to the same byte as reading advances it forward
                    fileSteam.Position--;

                    // Overwrite the byte with the encrypted version
                    fileSteam.WriteByte((byte)(b ^ FileData.FileXORKey[i % 4]));
                }
            }

            // Set the pending path
            PendingImportTempPath = tempFile.TempPath;

            return Task.FromResult(true);
        }

        #endregion
    }
}