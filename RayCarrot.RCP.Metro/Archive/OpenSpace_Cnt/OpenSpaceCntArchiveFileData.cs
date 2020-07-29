using ByteSizeLib;
using MahApps.Metro.IconPacks;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;
using RayCarrot.Binary;
using RayCarrot.Logging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

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
        public OpenSpaceCntArchiveFileData(OpenSpaceCntFileEntry fileEntry, OpenSpaceSettings settings, string directory)
        {
            Directory = directory;
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
        /// The format
        /// </summary>
        public uint Format { get; set; }

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public string FileDisplayInfo => String.Format(
            Resources.Archive_CNT_FileInfo, 
            Directory, 
            Width, Height, 
            ByteSize.FromBytes(FileEntry.Size), 
            FileEntry.Checksum == 0,
            FileEntry.FileXORKey.Any(x => x != 0), 
            IsTransparent, 
            $"0x{FileEntry.Pointer:x8}", 
            Mipmaps,
            Format);

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
        /// Gets the decoded contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="generator">The file generator</param>
        /// <param name="initilizeOnly">Indicates if the bytes should be retrieved for initialization only, in which case the bytes don't need to be returned</param>
        /// <returns>The contents of the file</returns>
        public byte[] GetDecodedFileBytes(Stream archiveFileStream, IDisposable generator, bool initilizeOnly)
        {
            // Don't read the bytes if it's only for initialization
            if (initilizeOnly)
                return null;

            // Get the bytes
            var bytes = GetEncodedFileBytes(archiveFileStream, generator);

            if (FileEntry.FileXORKey.Any(x => x != 0))
                // Decrypt the bytes
                bytes = new MultiXORDataEncoder(FileEntry.FileXORKey, true).Decode(bytes);

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
            return generator.CastTo<IArchiveFileGenerator<OpenSpaceCntFileEntry>>().GetBytes(FileEntry);
        }

        /// <summary>
        /// Gets the contents of the file with an option to deserialize mipmaps
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <returns>The deserialized file</returns>
        public OpenSpaceGFFile GetFileContent(byte[] fileBytes)
        {
            // Load the bytes into a memory stream
            using var stream = new MemoryStream(fileBytes);

            // Serialize the data
            var data = BinarySerializableHelpers.ReadFromStream<OpenSpaceGFFile>(stream, Settings, RCPServices.App.GetBinarySerializerLogger());

            // Make sure we read the entire file
            if (stream.Position != stream.Length)
                RL.Logger?.LogWarningSource($"The GF file {FileName} was not fully read");

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
            var file = GetFileContent(fileBytes);

            // Get the bitmap
            return file.GetRawBitmapData().GetBitmap();
        }

        /// <summary>
        /// Gets all images, including mipmaps, as bitmaps
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <returns>The images as a bitmaps</returns>
        public IEnumerable<Bitmap> GetBitmaps(byte[] fileBytes)
        {
            // Load the file
            var file = GetFileContent(fileBytes);

            // Get the bitmaps
            return file.GetRawBitmapDatas().Select(x => x.GetBitmap());
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
            var file = GetFileContent(fileBytes);

            // Set properties
            Height = file.Height;
            Width = file.Width;
            IsTransparent = file.GFPixelFormat.SupportsTransparency();
            Mipmaps = file.RealMipmapCount;
            Format = file.Format;

            // Get the thumbnail with the specified size
            return GetFileContent(fileBytes).GetRawBitmapData(width, (int)(Height / ((double)Width / width))).GetBitmap().ToImageSource();
        }

        /// <summary>
        /// Exports the file to the stream in the specified format
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="outputStream">The stream to export to</param>
        /// <param name="format">The file format to use</param>
        /// <returns>The task</returns>
        public void ExportFile(byte[] fileBytes, Stream outputStream, FileExtension format)
        {
            // Get the bitmap
            using var bmp = GetBitmap(fileBytes);

            // Get the format
            var imgFormat = ImageHelpers.GetImageFormat(format);

            // Save the file
            bmp.Save(outputStream, imgFormat);
        }

        /// <summary>
        /// Exports the mipmaps from the file
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="outputStreams">The function used to get the output streams for the mipmaps</param>
        /// <param name="format">The file extension to use</param>
        public void ExportMipmaps(byte[] fileBytes, Func<int, Stream> outputStreams, FileExtension format)
        {
            int index = 0;

            // Save each mipmap
            foreach (var bmp in GetBitmaps(fileBytes))
            {
                // Get the stream
                using var file = outputStreams(index);

                // Get the format
                var imgFormat = ImageHelpers.GetImageFormat(format);

                // Save the file
                bmp.Save(file, imgFormat);

                index++;
            }
        }

        /// <summary>
        /// Converts the import file data from the input stream to the output stream
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="inputStream">The input stream to import from</param>
        /// <param name="outputStream">The destination stream</param>
        /// <param name="format">The file format to use</param>
        public void ConvertImportData(byte[] fileBytes, Stream inputStream, Stream outputStream, FileExtension format)
        {
            // Load the bitmap
            using var bmp = new Bitmap(inputStream);

            // Load the current file
            OpenSpaceGFFile gf = GetFileContent(fileBytes);

            // IDEA: If bmp is not in supported format, then convert it?

            RawBitmapData rawBitmapData;

            // Get the bitmap lock
            using (var bmpLock = new BitmapLock(bmp))
            {
                // Get the raw bitmap data
                rawBitmapData = new RawBitmapData(bmp.Width, bmp.Height, bmpLock.Pixels, bmp.PixelFormat);

                // Force the new pixel format to be 888 or 8888 if set to do so
                if (RCPServices.Data.Archive_GF_ForceGF8888Import)
                    gf.GFPixelFormat = gf.GFPixelFormat.SupportsTransparency() ? OpenSpaceGFFormat.Format_32bpp_BGRA_8888 : OpenSpaceGFFormat.Format_24bpp_BGR_888;

                // Check if the format should be updated for transparency
                if (RCPServices.Data.Archive_GF_UpdateTransparency != Archive_GF_TransparencyMode.PreserveFormat)
                {
                    // NOTE: Only 24 and 32 bpp bitmaps are supported
                    // Check if the imported file is transparent
                    var isTransparent = bmp.PixelFormat switch
                    {
                        PixelFormat.Format32bppArgb => (RCPServices.Data.Archive_GF_UpdateTransparency == Archive_GF_TransparencyMode.UpdateBasedOnPixelFormat ||
                                                        bmpLock.UtilizesAlpha()),
                        PixelFormat.Format24bppRgb => false,
                        _ => (bool?)null
                    };

                    // NOTE: Currently only supported for formats with 3 or 4 channels
                    // Check if the format should be updated for transparency
                    if (gf.Channels >= 3 && isTransparent != null)
                        // Update the format
                        gf.GFPixelFormat = isTransparent.Value ? OpenSpaceGFFormat.Format_32bpp_BGRA_8888 : OpenSpaceGFFormat.Format_24bpp_BGR_888;
                }
            }

            // Import the bitmap
            gf.ImportFromBitmap(Settings, rawBitmapData, RCPServices.Data.Archive_GF_GenerateMipmaps);

            // Serialize the data to get the bytes
            BinarySerializableHelpers.WriteToStream(gf, outputStream, Settings, RCPServices.App.GetBinarySerializerLogger());
        }

        #endregion
    }
}