using ByteSizeLib;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archived file data for a UbiArt .ipk image file
    /// </summary>
    public class UbiArtIPKArchiveImageFileData : UbiArtIPKArchiveFileData, IArchiveImageFileData
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="baseOffset">The base offset to use when reading the files</param>
        public UbiArtIPKArchiveImageFileData(UbiArtIPKFile fileData, UbiArtSettings settings, int baseOffset) : base(fileData, settings, baseOffset)
        { }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the corresponding <see cref="PixelFormat"/> from an <see cref="Pfim.ImageFormat"/>
        /// </summary>
        /// <param name="imageFormat">The image format to use to get the pixel format</param>
        /// <returns>The pixel format</returns>
        protected PixelFormat GetPixelFormat(Pfim.ImageFormat imageFormat)
        {
            return imageFormat switch
            {
                Pfim.ImageFormat.Rgb24 => PixelFormat.Format24bppRgb,
                Pfim.ImageFormat.Rgba32 => PixelFormat.Format32bppArgb,
                Pfim.ImageFormat.R5g5b5 => PixelFormat.Format16bppRgb555,
                Pfim.ImageFormat.R5g6b5 => PixelFormat.Format16bppRgb565,
                Pfim.ImageFormat.R5g5b5a1 => PixelFormat.Format16bppArgb1555,
                Pfim.ImageFormat.Rgb8 => PixelFormat.Format8bppIndexed,
                _ => throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, null)
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the texture from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The texture</returns>
        public UbiArtTexture GetTexture(Stream archiveFileStream) => FileData.GetTexture(archiveFileStream, BaseOffset);

        /// <summary>
        /// Gets the image as a bitmap
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The image as a bitmap</returns>
        public Bitmap GetBitmap(Stream archiveFileStream)
        {
            // Get the texture
            var texture = GetTexture(archiveFileStream);

            // Read the texture data into a stream
            using var dataStream = new MemoryStream(texture.TextureData);

            // Read the texture as a DDS file
            using var dds = Pfim.Pfim.FromStream(dataStream);

            // Set the properties
            Width = dds.Width;
            Height = dds.Height;
            TextureVersion = texture.Version;

            // Get a pointer to the DDS data
            IntPtr dataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(dds.Data, 0);

            // Create and return a bitmap from the data
            return new Bitmap(dds.Width, dds.Height, dds.Stride, GetPixelFormat(dds.Format), dataPointer);
        }

        /// <summary>
        /// Gets all images, including mipmaps, as bitmaps
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The images as a bitmaps</returns>
        public IEnumerable<Bitmap> GetBitmaps(Stream archiveFileStream)
        {
            // Get the texture
            var texture = GetTexture(archiveFileStream);

            // Read the texture data into a stream
            using var dataStream = new MemoryStream(texture.TextureData);

            // Read the texture as a DDS file
            using var dds = Pfim.Pfim.FromStream(dataStream);

            // Get a pointer to the DDS data
            IntPtr dataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(dds.Data, 0);

            // Create and return the full bitmap from the data
            yield return new Bitmap(dds.Width, dds.Height, dds.Stride, GetPixelFormat(dds.Format), dataPointer);

            // Return every mipmap
            foreach (var mipmapOffset in dds.MipMaps)
            {
                // Get a pointer to the DDS data
                IntPtr mipmapDataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(dds.Data, mipmapOffset.DataOffset);

                // Create and return the full bitmap from the data
                yield return new Bitmap(mipmapOffset.Width, mipmapOffset.Height, mipmapOffset.Stride, GetPixelFormat(dds.Format), mipmapDataPointer);
            }
        }

        /// <summary>
        /// Gets the image as a bitmap with a specified width, while maintaining the aspect ratio
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="width">The width</param>
        /// <returns>The image as a bitmap</returns>
        public Bitmap GetBitmap(Stream archiveFileStream, int width)
        {
            // Return and resize the bitmap
            return GetBitmap(archiveFileStream).ResizeImage(width, (int)(Height / ((double)Width / width)));
        }

        /// <summary>
        /// Exports the file to the specified path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        public override Task ExportFileAsync(Stream archiveFileStream, FileSystemPath filePath, string fileFormat)
        {
            // Check if the file should be saved as its native format
            if (fileFormat == FileExtension)
            {
                return base.ExportFileAsync(archiveFileStream, filePath, fileFormat);
            }
            // Check if the file should be saved as DDS, the native format
            else if (fileFormat == ".dds")
            {
                // Open the file
                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the texture
                var texture = GetTexture(archiveFileStream);

                // Save the texture data
                file.Write(texture.TextureData, 0, (int)texture.TextureSize);
            }
            // Convert the file and save
            else
            {
                // Open the file
                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the bitmap
                using var bmp = GetBitmap(archiveFileStream);

                // Get the format
                var format = fileFormat switch
                {
                    ".bmp" => ImageFormat.Bmp,
                    ".png" => ImageFormat.Png,
                    ".jpeg" => ImageFormat.Jpeg,
                    ".jpg" => ImageFormat.Jpeg,
                    _ => throw new Exception($"The specified file format {fileFormat} is not supported")
                };

                // Save the file
                bmp.Save(file, format);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Exports the mipmaps from the file to the specified path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        public Task ExportMipmapsAsync(Stream archiveFileStream, FileSystemPath filePath, string fileFormat)
        {
            int index = 0;

            // Save each mipmap
            foreach (var bmp in GetBitmaps(archiveFileStream))
            {
                // Get the file path
                var mipmapFile = filePath;

                if (index > 0)
                    mipmapFile = mipmapFile.RemoveFileExtension().FullPath + $" ({index}){fileFormat}";

                // Open the file
                using var file = File.Open(mipmapFile, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the format
                var format = fileFormat switch
                {
                    ".bmp" => ImageFormat.Bmp,
                    ".png" => ImageFormat.Png,
                    ".jpeg" => ImageFormat.Jpeg,
                    ".jpg" => ImageFormat.Jpeg,
                    _ => throw new Exception($"The specified file format {fileFormat} is not supported")
                };

                // Save the file
                bmp.Save(file, format);

                index++;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Imports the file from the specified path to the <see cref="UbiArtIPKArchiveFileData.PendingImportTempPath"/> path
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="filePath">The path of the file to import</param>
        /// <returns>A value indicating if the file was successfully imported</returns>
        public override Task<bool> ImportFileAsync(Stream archiveFileStream, FileSystemPath filePath)
        {
            // Get the temporary file to save to, without disposing it
            var tempFile = new TempFile(false);

            // Check if the file is in the native format
            if (filePath.FileExtension.Equals(FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                // Copy the file
                RCFRCP.File.CopyFile(filePath, tempFile.TempPath, true);
            }
            // Import as DDS
            else if (filePath.FileExtension.Equals(".dds", StringComparison.InvariantCultureIgnoreCase))
            {
                // Get the file bytes
                var fileStream = File.OpenRead(filePath);

                // Get the current texture
                var texture = FileData.GetTexture(archiveFileStream, BaseOffset);

                // Load the DDS file
                using (var ddsFile = Pfim.Pfim.FromStream(fileStream))
                {
                    // Update texture fields
                    texture.Height = (ushort)ddsFile.Height;
                    texture.Width = (ushort)ddsFile.Width;
                }

                // Reset stream position
                fileStream.Position = 0;

                // Read the bytes from the stream
                texture.TextureData = fileStream.ReadRemainingBytes();

                // Set the size
                texture.TextureSize = (uint)texture.TextureData.Length;
                texture.TextureSize2 = (uint)texture.TextureData.Length;

                // Write the texture to the temp file
                new UbiArtTextureSerializer(Settings).Serialize(tempFile.TempPath, texture);
            }
            else
            {
                throw new Exception("Invalid file extension");
            }

            // Set the pending path
            PendingImportTempPath = tempFile.TempPath;

            return Task.FromResult(true);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public override string FileDisplayInfo => String.Format(
            Resources.Archive_IPK_ImageFileInfo,
            Directory,
            Width, Height,
            TextureVersion,
            FileData.IsCompressed,
            new ByteSize(FileData.Size),
            new ByteSize(FileData.CompressedSize),
            FileData.Offset + BaseOffset);

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public override string[] SupportedImportFileExtensions => new string[]
        {
            ".dds",
            FileExtension
        };

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public override string[] SupportedExportImportFileExtensions => new string[]
        {
            ".dds",
            ".png",
            FileExtension,
            ".jpg",
            ".jpeg",
            ".bmp",
        };

        /// <summary>
        /// The supported file formats for exporting mipmaps
        /// </summary>
        public string[] SupportedMipmapExportFileExtensions => new[]
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
        };

        /// <summary>
        /// Indicates if the image has mipmaps
        /// </summary>
        public bool HasMipmaps => true;

        /// <summary>
        /// The image height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The image width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The texture version
        /// </summary>
        public uint TextureVersion { get; set; }

        #endregion
    }
}