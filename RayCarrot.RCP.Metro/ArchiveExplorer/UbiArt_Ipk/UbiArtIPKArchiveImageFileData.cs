using ByteSizeLib;
using ImageMagick;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

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
        /// <param name="fileEntry">The file data</param>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="baseOffset">The base offset to use when reading the files</param>
        public UbiArtIPKArchiveImageFileData(UbiArtIPKFileEntry fileEntry, UbiArtSettings settings, uint baseOffset) : base(fileEntry, settings, baseOffset)
        { }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Indicates if the file extension is the same as the file's native format
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it's the same</returns>
        protected bool IsNativeFormat(string fileExtension) => fileExtension.Equals(FileExtension.PrimaryFileExtension, StringComparison.InvariantCultureIgnoreCase);
        
        /// <summary>
        /// Indicates if the file extension is the same as the TEX extension
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it's the same</returns>
        protected bool IsTEXFormat(string fileExtension) => fileExtension.Equals(TEXFileExtension, StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the .tex file data
        /// </summary>
        /// <param name="bytes">The bytes to get the data from</param>
        /// <returns>The .tex file data</returns>
        protected UbiArtTEXFile GetTexFile(byte[] bytes)
        {
            // Create a memory stream
            using var memoryStream = new MemoryStream(bytes);

            return UbiArtTEXFile.GetSerializer(Settings).Deserialize(memoryStream);
        }

        /// <summary>
        /// Gets the texture data from the stream
        /// </summary>
        /// <param name="bytes">The file bytes</param>
        /// <returns>The texture data</returns>
        protected byte[] GetTextureData(byte[] bytes)
        {
            if (UsesTexWrapper)
            {
                // Get the texture
                var texture = GetTexFile(bytes);

                // Return the texture data
                return texture.TextureData;
            }
            else
            {
                // Return the bytes
                return bytes;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the data for the file
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        public override void InitializeData(byte[] fileBytes)
        {
            if (HasInitializedData)
                return;

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
                // Get the texture
                var tex = GetTexFile(fileBytes);

                // Get the format
                magic = GetUInt32(tex.TextureData, 0);
            }
            else
            {
                // Get the format
                magic = GetUInt32(fileBytes, 0);
            }

            // Find the format which matches the magic header
            TextureFormat = EnumHelpers.GetValues<UbiArtTextureFormat>().FindItem(x => x.GetAttribute<FileFormatInfoAttribute>().MagicHeader == magic);

            // Set the file extension
            if (TextureFormat != UbiArtTextureFormat.Unknown)
                FileExtension = new FileExtension(TextureFormat.GetAttribute<FileFormatInfoAttribute>().FileExtension);

            // Set the supported file extensions
            var supportedImportFileExtensions = new List<FileExtension>();
            var supportedExportFileExtensions = new List<FileExtension>();

            // Handle supported formats
            if (IsFormatSupported)
            {
                // Get the extensions
                var extensions = ImageHelpers.GetSupportedMagickExtensions();

                // If the current one is not included, add it
                if (!extensions.Any(x => x.Equals(FileExtension.PrimaryFileExtension, StringComparison.InvariantCultureIgnoreCase)))
                {
                    supportedImportFileExtensions.Add(FileExtension);
                    supportedExportFileExtensions.Add(FileExtension);
                }

                // Add common extensions
                supportedImportFileExtensions.AddRange(extensions.Select(x => new FileExtension(x)));
                supportedExportFileExtensions.AddRange(extensions.Select(x => new FileExtension(x)));
            }
            // Handle unsupported formats
            else
            {
                supportedImportFileExtensions.Add(FileExtension);
                supportedExportFileExtensions.Add(FileExtension);
            }

            if (UsesTexWrapper)
            {
                supportedImportFileExtensions.Add(new FileExtension(TEXFileExtension));
                supportedExportFileExtensions.Add(new FileExtension(TEXFileExtension));
            }

            // Set the supported extensions
            SupportedImportFileExtensions = supportedImportFileExtensions.ToArray();
            SupportedExportFileExtensions = supportedExportFileExtensions.ToArray();

            RCFCore.Logger?.LogTraceSource($"{FileName} has been initialized");

            HasInitializedData = true;
        }

        /// <summary>
        /// Gets the image
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="skipMipmaps">Indicates if reading mipmaps should be skipped</param>
        /// <returns>The image</returns>
        public MagickImage GetImage(byte[] fileBytes, bool skipMipmaps = false)
        {
            // Initialize the data
            InitializeData(fileBytes);

            // Get the bytes from the TEX wrapper if available
            if (UsesTexWrapper)
            {
                // Get the texture
                var texture = GetTexFile(fileBytes);

                // Get the size in case it can't be read from the file
                Width = texture.Width;
                Height = texture.Height;

                // Set the texture data
                fileBytes = texture.TextureData;
            }

            // Skip unsupported formats
            if (!SupportedFormats.Contains(TextureFormat))
                return null;

            IReadDefines def = null;

            if (TextureFormat == UbiArtTextureFormat.DDS && skipMipmaps)
            {
                def = new DdsReadDefines()
                {
                    SkipMipmaps = true
                };
            }

            // Create the read settings
            var settings = new MagickReadSettings();

            // Add defines if available
            if (def != null)
                settings.Defines = def;

            // Create the image
            var img = new MagickImage(fileBytes, settings);

            // Set the properties
            Width = img.Width;
            Height = img.Height;

            // Return the image
            return img;
        }

        /// <summary>
        /// Gets the image as an image source with a specified width, while maintaining the aspect ratio
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="width">The width</param>
        /// <returns>The image as an image source</returns>
        public ImageSource GetThumbnail(byte[] fileBytes, int width)
        {
            // Get the image
            using var img = GetImage(fileBytes, true);

            if (img == null || Height == null || Width == null)
                return null;

            // Resize the image
            img.Resize(width, (int)(Height / ((double)Width / width)));

            // Return the image
            return img.ToBitmapSource();
        }

        /// <summary>
        /// Exports the file to the specified path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        public override Task ExportFileAsync(byte[] fileBytes, FileSystemPath filePath, string fileFormat)
        {
            RCFCore.Logger?.LogInformationSource($"An IPK archive file is being exported as {fileFormat}");

            // Check if the file should be saved as a TEX file, in which case use the native, unmodified file
            if (IsTEXFormat(fileFormat))
            {
                // Export the file as its native format
                return base.ExportFileAsync(fileBytes, filePath, fileFormat);
            }
            // Check if the file should be saved as the native format
            else if (IsNativeFormat(fileFormat))
            {
                // Open the file
                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the texture data
                var textureData = GetTextureData(fileBytes);

                // Save the texture data
                file.Write(textureData, 0, textureData.Length);
            }
            // Convert the file and save
            else
            {
                // Open the file
                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // Get the image
                using var img = GetImage(fileBytes);

                // Set the format
                img.Format = ImageHelpers.GetMagickFormat(fileFormat);

                // Save the file
                img.Write(file);
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
        public Task ExportMipmapsAsync(byte[] fileBytes, FileSystemPath filePath, string fileFormat) => throw new Exception("IPK files do not support mipmaps");

        /// <summary>
        /// Imports the file from the specified path to the <see cref="UbiArtIPKArchiveFileData.PendingImportTempPath"/> path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path of the file to import</param>
        /// <returns>A value indicating if the file was successfully imported</returns>
        public override Task<bool> ImportFileAsync(byte[] fileBytes, FileSystemPath filePath)
        {
            RCFCore.Logger?.LogInformationSource($"An IPK archive file is being imported as {filePath.FileExtension}");

            // Get the temporary file to save to, without disposing it
            var tempFile = new TempFile(false);

            // Check if the file is in the TEX format or in the native format, thus not needing to be converted
            if (IsTEXFormat(filePath.FileExtension) || filePath.FileExtensions.JoinItems(String.Empty) == new FileExtension(FileName).FileExtensions)
            {
                // Copy the file
                RCFRCP.File.CopyFile(filePath, tempFile.TempPath, true);
            }
            // Import as a standard image format
            else if (!UsesTexWrapper)
            {
                // Read the file bytes
                var bytes = File.ReadAllBytes(filePath);

                // If it's not in the native format and supported, convert it
                if (IsFormatSupported && !IsNativeFormat(filePath.FileExtension))
                {
                    // Load the file
                    using var img = new MagickImage(bytes)
                    {
                        // Set the format
                        Format = ImageHelpers.GetMagickFormat(TextureFormat.GetAttribute<FileFormatInfoAttribute>()
                            .FileExtension)
                    };
                    
                    // Update the bytes
                    bytes = img.ToByteArray();
                }

                // Write the bytes
                File.WriteAllBytes(tempFile.TempPath, bytes);
            }
            // Import as a standard image format in a TEX wrapper
            else
            {
                // Read the file bytes
                var bytes = File.ReadAllBytes(filePath);

                // Get the current texture
                var texture = GetTexFile(fileBytes);

                // Update fields if the format is supported
                if (IsFormatSupported)
                {
                    // Load the file
                    using var img = new MagickImage(bytes);

                    // Check if the file is in the native format
                    var isNative = IsNativeFormat(filePath.FileExtension);

                    // If it's not in the native format, convert it
                    if (!isNative)
                    {
                        // Set the format
                        img.Format = ImageHelpers.GetMagickFormat(TextureFormat.GetAttribute<FileFormatInfoAttribute>().FileExtension);

                        // Update the bytes
                        bytes = img.ToByteArray();
                    }

                    // Update texture fields
                    texture.Height = (ushort)img.Height;
                    texture.Width = (ushort)img.Width;
                }

                // Set the bytes
                texture.TextureData = bytes;

                // TODO: Figure out what the values are on Wii U where they don't match the actual size
                // On PS Vita the values are always 0, so keep them that way
                if (Settings.Platform != UbiArtPlatform.PSVita)
                {
                    // Set the length
                    texture.TextureSize = (uint)texture.TextureData.Length;
                    texture.TextureSize2 = (uint)texture.TextureData.Length;
                }

                // Write the texture to the temp file
                UbiArtTEXFile.GetSerializer(Settings).Serialize(tempFile.TempPath, texture);
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
        protected bool HasInitializedData { get; set; }

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
            FileExtension.FileExtensions,
            FileEntry.IsCompressed,
            ByteSize.FromBytes(FileEntry.Size),
            ByteSize.FromBytes(FileEntry.CompressedSize),
            FileEntry.Offsets.First() + BaseOffset);

        /// <summary>
        /// The default icon to use for this file
        /// </summary>
        public override PackIconMaterialKind IconKind => PackIconMaterialKind.FileImageOutline;

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public override FileExtension[] SupportedImportFileExtensions { get; set; }

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public override FileExtension[] SupportedExportFileExtensions { get; set; }

        /// <summary>
        /// The supported file formats for exporting mipmaps
        /// </summary>
        public FileExtension[] SupportedMipmapExportFileExtensions => null;

        /// <summary>
        /// Indicates if the image has mipmaps
        /// </summary>
        public bool HasMipmaps => false;

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

        /// <summary>
        /// Indicates if the current format is supported
        /// </summary>
        public bool IsFormatSupported => SupportedFormats.Contains(TextureFormat);

        /// <summary>
        /// Gets the currently supported image formats
        /// </summary>
        public UbiArtTextureFormat[] SupportedFormats => new UbiArtTextureFormat[]
        {
            UbiArtTextureFormat.DDS,
            UbiArtTextureFormat.PNG,
        };

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
            [FileFormatInfo(".unk", 0x00000000)]
            Unknown,

            /// <summary>
            /// DDS (default)
            /// </summary>
            [FileFormatInfo(".dds", 0x44445320)]
            DDS,

            /// <summary>
            /// GXT (used on PlayStation Vita)
            /// </summary>
            [FileFormatInfo(".gxt", 0x47585400)]
            GXT,

            /// <summary>
            /// GFX2 (used on Wii U)
            /// </summary>
            [FileFormatInfo(".gtx", 0x47667832)]
            GFX2,

            /// <summary>
            /// PVR (used on iOS)
            /// </summary>
            [FileFormatInfo(".pvr", 0x50565203)]
            PVR,

            /// <summary>
            /// GNF (used on PS4)
            /// </summary>
            [FileFormatInfo(".gnf", 0x474E4620)]
            GNF,

            /// <summary>
            /// PNG
            /// </summary>
            [FileFormatInfo(".png", 0x89504E47)]
            PNG,
        }

        #endregion
    }
}