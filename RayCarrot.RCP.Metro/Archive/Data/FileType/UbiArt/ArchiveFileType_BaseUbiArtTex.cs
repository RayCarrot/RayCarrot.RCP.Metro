using System.IO;
using System.Linq;
using ImageMagick;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A base UbiArt texture file type
    /// </summary>
    public abstract class ArchiveFileType_BaseUbiArtTex : ArchiveFileType_Image
    {
        /// <summary>
        /// The display name for the file type
        /// </summary>
        public override string TypeDisplayName => $"TEX ({Format})";

        /// <summary>
        /// Indicates if the specified manager supports files of this type
        /// </summary>
        /// <param name="manager">The manager to check</param>
        /// <returns>True if supported, otherwise false</returns>
        public override bool IsSupported(IArchiveDataManager manager) => manager.SerializerSettings is UbiArtSettings;

        /// <summary>
        /// Indicates if a file with the specifies file extension is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public override bool IsOfType(FileExtension fileExtension) => false;

        /// <summary>
        /// Indicates if a file with the specifies file extension and data is of this type
        /// </summary>
        /// <param name="fileExtension">The file extension to check</param>
        /// <param name="inputStream">The file data to check</param>
        /// <param name="manager">The manager</param>
        /// <returns>True if it is of this type, otherwise false</returns>
        public override bool IsOfType(FileExtension fileExtension, Stream inputStream, IArchiveDataManager manager)
        {
            if (fileExtension != new FileExtension(".tga.ckd") && fileExtension != new FileExtension(".png.ckd"))
                return false;

            // Set the Stream position
            ReadTEXHeader(inputStream, manager);

            // Use a reader
            using var reader = new Reader(inputStream, manager.SerializerSettings.Endian, true);

            // Get the magic header
            var magic = reader.ReadUInt32();

            // Check if it matches the magic
            return magic == FormatMagic;
        }

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        public override FileExtension[] ImportFormats => IsFormatSupported ? new FileExtension[]
        {
            Format
        }.Concat(base.ImportFormats.Where(x => x != Format)).ToArray() : new FileExtension[0];

        /// <summary>
        /// The supported formats to export to
        /// </summary>
        public override FileExtension[] ExportFormats => IsFormatSupported ? new FileExtension[]
        {
            Format
        }.Concat(base.ExportFormats.Where(x => x != Format)).ToArray() : new FileExtension[]
        {
            Format
        };

        /// <summary>
        /// Loads the thumbnail and display info for the file
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="fileExtension">The file extension</param>
        /// <param name="width">The thumbnail width</param>
        /// <param name="manager">The manager</param>
        /// <returns>The thumbnail data</returns>
        public override ArchiveFileThumbnailData LoadThumbnail(ArchiveFileStream inputStream, FileExtension fileExtension, int width, IArchiveDataManager manager)
        {
            // Only load thumbnails for supported formats
            if (!IsFormatSupported)
                return new ArchiveFileThumbnailData(null, new DuoGridItemViewModel[0]);
            else
                return base.LoadThumbnail(inputStream, fileExtension, width, manager);
        }

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="inputFormat">The format to convert from</param>
        /// <param name="outputFormat">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        public override void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, Stream inputStream, Stream outputStream,
            IArchiveDataManager manager)
        {
            // Check if it's the native format
            if (outputFormat == Format)
            {
                // Set the start position
                ReadTEXHeader(inputStream, manager);

                // Copy the image data
                inputStream.CopyTo(outputStream);
            }
            else
            {
                // Convert the image normally
                base.ConvertTo(inputFormat, outputFormat, inputStream, outputStream, manager);
            }
        }

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="inputFormat">The format to convert from</param>
        /// <param name="outputFormat">The format to convert to</param>
        /// <param name="currentFileStream">The current file stream</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        public override void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream,
            Stream inputStream, Stream outputStream, IArchiveDataManager manager)
        {
            // Get the current TEX data
            var tex = ReadTEXHeader(currentFileStream.Stream, manager);

            // If there's no TEX header we handle the image data directly
            if (tex == null)
            {
                if (outputFormat == Format)
                    inputStream.CopyTo(outputStream);
                else
                    ConvertFrom(inputFormat, MagickFormat, inputStream, outputStream);
            }
            else
            {
                // Get the image in specified format
                using var img = new MagickImage(inputStream, MagickFormatInfo.Create(inputFormat.FileExtensions).Format);

                // Get the image bytes
                var bytes = img.ToByteArray();

                // Create a TEX file
                var texFile = new UbiArtTEXFileData()
                {
                    TexHeader = tex,
                    ImageData = bytes
                };

                // Update the TEX header
                tex.Height = (ushort)img.Height;
                tex.Width = (ushort)img.Width;
                // TODO: Figure out what the values are on Wii U where they don't match the actual size
                tex.TextureSize = (uint)bytes.Length;
                tex.TextureSize2 = (uint)bytes.Length;

                // Write the TEX file
                BinarySerializableHelpers.WriteToStream(texFile, outputStream, manager.SerializerSettings, RCPServices.App.GetBinarySerializerLogger());
            }
        }

        /// <summary>
        /// Gets an image from the file data
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="format">The file format</param>
        /// <param name="manager">The manager to check</param>
        /// <returns>The image</returns>
        protected override MagickImage GetImage(Stream inputStream, FileExtension format, IArchiveDataManager manager)
        {
            // Set the Stream position
            ReadTEXHeader(inputStream, manager);

            // Return the image
            return new MagickImage(inputStream);
        }

        /// <summary>
        /// Gets the format to display the image as
        /// </summary>
        /// <param name="ext">The file extension</param>
        /// <returns>The file format display name</returns>
        public override string GetFormat(FileExtension ext) => Format.DisplayName;

        /// <summary>
        /// Reads the TEX header if there is one
        /// </summary>
        /// <param name="inputStream">The input stream</param>
        /// <param name="manager">The manager</param>
        /// <returns>The TEX header, if available</returns>
        protected UbiArtTEXData ReadTEXHeader(Stream inputStream, IArchiveDataManager manager)
        {
            // Use a reader
            using var reader = new Reader(inputStream, manager.SerializerSettings.Endian, true);

            // Check if it's in a TEX wrapper
            inputStream.Position = 4;
            var usesTexWrapper = reader.ReadUInt32() == TEXHeader;

            // Reset the position
            inputStream.Position = 0;

            // If it uses a TEX wrapper we need to serialize the header
            if (usesTexWrapper)
            {
                // Serialize the header
                return BinarySerializableHelpers.ReadFromStream<UbiArtTEXData>(inputStream, manager.SerializerSettings, RCPServices.App.GetBinarySerializerLogger());
            }

            return null;
        }

        /// <summary>
        /// The header for a TEX wrapper
        /// </summary>
        protected const uint TEXHeader = 0x54455800;

        /// <summary>
        /// The format
        /// </summary>
        protected abstract FileExtension Format { get; }

        /// <summary>
        /// The magick format
        /// </summary>
        protected abstract MagickFormat MagickFormat { get; }

        /// <summary>
        /// The magic header for the format
        /// </summary>
        protected abstract uint FormatMagic { get; }

        /// <summary>
        /// Indicates if the format is fully supported and can be read as an image
        /// </summary>
        protected abstract bool IsFormatSupported { get; }
    }
}