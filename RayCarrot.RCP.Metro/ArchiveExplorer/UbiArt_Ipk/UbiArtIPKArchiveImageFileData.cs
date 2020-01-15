using ByteSizeLib;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        public UbiArtIPKArchiveImageFileData(UbiArtIPKFile fileData, UbiArtSettings settings, uint baseOffset) : base(fileData, settings, baseOffset)
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

        /// <summary>
        /// Indicates if the file extension is the same as the file's native format
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it's the same</returns>
        protected bool IsNativeFormat(string fileExtension) => fileExtension.Equals(FileExtension, StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the texture data from the stream
        /// </summary>
        /// <param name="bytes">The file bytes</param>
        /// <returns>The texture data</returns>
        protected byte[] GetTextureData(byte[] bytes)
        {
            if (UsesTexWrapper)
            {
                // Create a memory stream
                using var memoryStream = new MemoryStream(bytes);

                // Get the texture
                var texture = FileData.GetTEXFile(memoryStream);

                // Return the texture data
                return texture.TextureData;
            }
            else
            {
                // Return the bytes
                return bytes;
            }
        }

        /// <summary>
        /// Initializes the data for the file
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        protected void InitializeData(byte[] fileBytes)
        {
            static uint GetUInt32(IEnumerable<byte> buffer, int startIndex)
            {
                // Convert the first 4 bytes, reversed, to an unsigned Int32 and return
                return BitConverter.ToUInt32(buffer.Skip(startIndex).Take(4).Reverse().ToArray(), 0);
            }

            // Check if the file is wrapped in a TEX file
            UsesTexWrapper = GetUInt32(fileBytes, 4) == 0x54455800;

            // Get the magic header
            uint magic;

            // Get the texture format
            if (UsesTexWrapper)
            {
                // Get the bytes and load them into a memory stream
                using var memoryStream = new MemoryStream(fileBytes);

                // Get the texture
                var tex = FileData.GetTEXFile(memoryStream);

                // Get the format
                magic = GetUInt32(tex.TextureData, 0);
            }
            else
            {
                // Get the format
                magic = GetUInt32(fileBytes, 0);
            }

            // Find the format which matches the magic header
            TextureFormat = UbiArtTextureFormat.Unknown.GetValues().
                FindItem(x => x.GetAttribute<TextureFormatInfoAttribute>().MagicHeader == magic);

            // Set the file extension
            FileExtension = TextureFormat.GetAttribute<TextureFormatInfoAttribute>().FileExtension;

            // Set the supported file extensions
            var supportedMipmapExportFileExtensions = new List<string>();
            var supportedImportFileExtensions = new List<string>();
            var supportedExportFileExtensions = new List<string>();

            switch (TextureFormat)
            {
                case UbiArtTextureFormat.DDS:

                    // DDS files have mipmaps
                    HasMipmaps = true;

                    supportedMipmapExportFileExtensions.AddRange(new[]
                    {
                        ".png",
                        ".jpg",
                        ".jpeg",
                        ".bmp",
                    });

                    supportedImportFileExtensions.AddRange(new[]
                    {
                        ".dds",
                    });

                    supportedExportFileExtensions.AddRange(new[]
                    {
                        ".dds",
                        ".png",
                        ".jpg",
                        ".jpeg",
                        ".bmp",
                    });
                    break;

                case UbiArtTextureFormat.PNG:

                    HasMipmaps = false;

                    supportedImportFileExtensions.AddRange(new[]
                    {
                        ".png",
                        ".jpg",
                        ".jpeg",
                        ".bmp",
                    });

                    supportedExportFileExtensions.AddRange(new[]
                    {
                        ".png",
                        ".jpg",
                        ".jpeg",
                        ".bmp",
                    });

                    break;

                case UbiArtTextureFormat.GXT:
                case UbiArtTextureFormat.GFX2:
                case UbiArtTextureFormat.Unknown:

                    HasMipmaps = false;

                    supportedImportFileExtensions.AddRange(new[]
                    {
                        FileExtension
                    });

                    supportedExportFileExtensions.AddRange(new[]
                    {
                        FileExtension
                    });

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(TextureFormat));
            }

            if (UsesTexWrapper)
                supportedImportFileExtensions.Add(TEXFileExtension);

            if (UsesTexWrapper)
                supportedExportFileExtensions.Add(TEXFileExtension);

            // Set the supported extensions
            SupportedMipmapExportFileExtensions = supportedMipmapExportFileExtensions.ToArray();
            SupportedImportFileExtensions = supportedImportFileExtensions.ToArray();
            SupportedExportFileExtensions = supportedExportFileExtensions.ToArray();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the data for the file
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        public override void InitializeData(Stream archiveFileStream)
        {
            if (HasInitializedData)
                return;

            InitializeData(GetFileBytes(archiveFileStream));

            HasInitializedData = true;
        }

        /// <summary>
        /// Gets the image as a bitmap
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The image as a bitmap</returns>
        public Bitmap GetBitmap(Stream archiveFileStream)
        {
            // Get the file bytes
            var bytes = GetFileBytes(archiveFileStream);

            // Initialize the data
            InitializeData(bytes);

            // Get the bytes from the TEX wrapper if available
            if (UsesTexWrapper)
            {
                // Create a memory stream
                using var memoryStream = new MemoryStream(bytes);

                // Get the texture
                var texture = FileData.GetTEXFile(memoryStream);

                // Get the size in case it can't be read from the file
                Width = texture.Width;
                Height = texture.Height;

                // Set the texture data
                bytes = texture.TextureData;
            }

            if (TextureFormat == UbiArtTextureFormat.DDS)
            {
                // Read the texture data into a stream
                using var dataStream = new MemoryStream(bytes);

                // Read the texture as a DDS file
                using var dds = Pfim.Pfim.FromStream(dataStream);

                // Set the properties
                Width = dds.Width;
                Height = dds.Height;

                // Get a pointer to the DDS data
                IntPtr dataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(dds.Data, 0);

                // Create and return a bitmap from the data
                return new Bitmap(dds.Width, dds.Height, dds.Stride, GetPixelFormat(dds.Format), dataPointer);
            }
            else if (TextureFormat == UbiArtTextureFormat.PNG)
            {
                // Load the bytes into a memory stream
                using var bytesMemory = new MemoryStream(bytes);

                // Get the bitmap
                var bmp = new Bitmap(bytesMemory);

                // Set the properties
                Width = bmp.Width;
                Height = bmp.Height;

                // Return the bitmap
                return bmp;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all images, including mipmaps, as bitmaps
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The images as a bitmaps</returns>
        public IEnumerable<Bitmap> GetBitmaps(Stream archiveFileStream)
        {
            if (TextureFormat != UbiArtTextureFormat.DDS)
                throw new Exception("Only DDS files support mipmaps");

            // Get the texture data
            var textureData = GetTextureData(GetFileBytes(archiveFileStream));

            // Read the texture data into a stream
            using var dataStream = new MemoryStream(textureData);

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
        public Bitmap GetThumbnail(Stream archiveFileStream, int width)
        {
            if (Height == null || Width == null)
                return null;

            // Return and resize the bitmap
            return GetBitmap(archiveFileStream)?.ResizeImage(width, (int)(Height / ((double)Width / width)));
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
            // Check if the file should be saved as a TEX file, in which case use the native, unmodified file
            if (fileFormat == TEXFileExtension)
            {
                // Export the file as its native format
                return base.ExportFileAsync(archiveFileStream, filePath, fileFormat);
            }
            // Check if the file should be saved as the native format
            else if (IsNativeFormat(fileFormat))
            {
                // Open the file
                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the texture data
                var textureData = GetTextureData(GetFileBytes(archiveFileStream));

                // Save the texture data
                file.Write(textureData, 0, textureData.Length);
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

            // Check if the file is in the TEX format
            if (filePath.FileExtension.Equals(TEXFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                // Copy the file
                RCFRCP.File.CopyFile(filePath, tempFile.TempPath, true);
            }
            // Check if the file is in the native format
            else if (IsNativeFormat(filePath.FileExtension))
            {
                // Get the file bytes
                using var fileStream = File.OpenRead(filePath);

                // Get the current texture
                var texture = FileData.GetTEXFile(archiveFileStream, BaseOffset);

                switch (TextureFormat)
                {
                    case UbiArtTextureFormat.DDS:

                        // Load the DDS file
                        using (var ddsFile = Pfim.Pfim.FromStream(fileStream))
                        {
                            // Update texture fields
                            texture.Height = (ushort)ddsFile.Height;
                            texture.Width = (ushort)ddsFile.Width;
                        }

                        break;

                    case UbiArtTextureFormat.PNG:

                        // Load the PNG file
                        using (var bmp = new Bitmap(fileStream))
                        {
                            // Update texture fields
                            texture.Height = (ushort)bmp.Height;
                            texture.Width = (ushort)bmp.Width;
                        }

                        break;

                    case UbiArtTextureFormat.GXT:
                    case UbiArtTextureFormat.GFX2:
                    case UbiArtTextureFormat.PVR:
                    case UbiArtTextureFormat.GNF:
                    case UbiArtTextureFormat.Unknown:
                    default:
                        // Do nothing
                        break;
                }

                // Reset stream position
                fileStream.Position = 0;

                // Read the bytes from the stream
                texture.TextureData = fileStream.ReadRemainingBytes();

                // TODO: The size doesn't match for one of the platforms - why? Wii U? On Vita it's 0. Always?
                // Set the size
                texture.TextureSize = (uint)texture.TextureData.Length;
                texture.TextureSize2 = (uint)texture.TextureData.Length;

                // Write the texture to the temp file
                new UbiArtTextureSerializer(Settings).Serialize(tempFile.TempPath, texture);
            }
            // Import as standard image format
            else
            {
                switch (TextureFormat)
                {
                    case UbiArtTextureFormat.PNG:

                        // Read the file into a bitmap
                        using (var bmp = new Bitmap(filePath))
                        {
                            // Save the bitmap to the temp path as PNG
                            bmp.Save(tempFile.TempPath, ImageFormat.Png);
                        }

                        break;

                    case UbiArtTextureFormat.DDS:
                    case UbiArtTextureFormat.GXT:
                    case UbiArtTextureFormat.GFX2:
                    case UbiArtTextureFormat.PVR:
                    case UbiArtTextureFormat.GNF:
                    case UbiArtTextureFormat.Unknown:
                    default:
                        throw new Exception("The specified format does not support importing from standard image formats");
                }
            }

            // Set the pending path
            PendingImportTempPath = tempFile.TempPath;

            return Task.FromResult(true);
        }

        #endregion

        #region Protected Constants

        /// <summary>
        /// The file extension to use for TEX files
        /// </summary>
        protected const string TEXFileExtension = ".tex";

        #endregion

        #region Protected Properties

        /// <summary>
        /// Indicates if the data has been initialized
        /// </summary>
        public bool HasInitializedData { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public override string FileDisplayInfo => String.Format(
            Resources.Archive_IPK_ImageFileInfo,
            Directory,
            Width?.ToString() ?? "N/A", Height?.ToString() ?? "N/A",
            UsesTexWrapper,
            FileExtension,
            FileData.IsCompressed,
            new ByteSize(FileData.Size),
            new ByteSize(FileData.CompressedSize),
            FileData.Offset + BaseOffset);

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public override string[] SupportedImportFileExtensions { get; set; }

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public override string[] SupportedExportFileExtensions { get; set; }

        /// <summary>
        /// The supported file formats for exporting mipmaps
        /// </summary>
        public string[] SupportedMipmapExportFileExtensions { get; set; }

        /// <summary>
        /// Indicates if the image has mipmaps
        /// </summary>
        public bool HasMipmaps { get; set; }

        /// <summary>
        /// The image height
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// The image width
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// The texture format for the image file
        /// </summary>
        public UbiArtTextureFormat TextureFormat { get; set; }

        /// <summary>
        /// Indicates if the texture is wrapped in a TEX file
        /// </summary>
        public bool UsesTexWrapper { get; set; }

        #endregion

        #region Enums

        /// <summary>
        /// The supported UbiArt texture formats
        /// </summary>
        public enum UbiArtTextureFormat
        {
            /// <summary>
            /// Unknown
            /// </summary>
            [TextureFormatInfo(".unk", 0x00000000)]
            Unknown,

            /// <summary>
            /// DDS (default)
            /// </summary>
            [TextureFormatInfo(".dds", 0x44445320)]
            DDS,

            /// <summary>
            /// GXT (used on PlayStation Vita)
            /// </summary>
            [TextureFormatInfo(".gxt", 0x47585400)]
            GXT,

            /// <summary>
            /// GFX2 (used on Wii U)
            /// </summary>
            [TextureFormatInfo(".gtx", 0x47667832)]
            GFX2,

            /// <summary>
            /// PVR (used on iOS)
            /// </summary>
            [TextureFormatInfo(".pvr", 0x50565203)]
            PVR,

            /// <summary>
            /// GNF (used on PS4)
            /// </summary>
            [TextureFormatInfo(".gnf", 0x474E4620)]
            GNF,

            /// <summary>
            /// PNG
            /// </summary>
            [TextureFormatInfo(".png", 0x89504E47)]
            PNG,
        }

        /// <summary>
        /// Specifies information for a <see cref="UbiArtTextureFormat"/>
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public sealed class TextureFormatInfoAttribute : Attribute
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="fileExtension">The file extension</param>
            /// <param name="magicHeader">The magic header</param>
            public TextureFormatInfoAttribute(string fileExtension, uint magicHeader)
            {
                FileExtension = fileExtension;
                MagicHeader = magicHeader;
            }

            /// <summary>
            /// The file extension
            /// </summary>
            public string FileExtension { get; }

            /// <summary>
            /// The magic header
            /// </summary>
            public uint MagicHeader { get; }
        }

        #endregion
    }
}