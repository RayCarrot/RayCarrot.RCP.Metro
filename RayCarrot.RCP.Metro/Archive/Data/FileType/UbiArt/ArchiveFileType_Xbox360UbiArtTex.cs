using ImageMagick;
using RayCarrot.Binary;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A Xbox 360 UbiArt texture file type (Xbox 360)
    /// </summary>
    public class ArchiveFileType_Xbox360UbiArtTex : ArchiveFileType_BaseUbiArtTex
    {
        /// <summary>
        /// Indicates if the format is fully supported and can be read as an image
        /// </summary>
        protected override bool IsFormatSupported => true;

        /// <summary>
        /// The supported formats to import from
        /// </summary>
        public override FileExtension[] ImportFormats => new FileExtension[0];

        /// <summary>
        /// The format
        /// </summary>
        protected override FileExtension Format => new FileExtension(".dds_xbox");

        public override bool IsOfType(Stream inputStream, IArchiveDataManager manager, UbiArtTEXData tex)
        {
            var settings = manager.SerializerSettings.CastTo<UbiArtSettings>();

            // TODO: Find better way to check this
            return settings.Platform == Platform.Xbox360;
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

            // Serialize data
            var imgData = BinarySerializableHelpers.ReadFromStream<UbiArtXbox360Texture>(inputStream, manager.SerializerSettings, logger: RCPServices.App.GetBinarySerializerLogger());

            // Get the untiled image data
            var untiledImgData = imgData.Untile(true);

            DDSParser.DDSStruct header = new DDSParser.DDSStruct
            {
                pixelformat = new DDSParser.DDSStruct.pixelformatstruct()
                {
                    rgbbitcount = 32
                },
                width = (uint)imgData.Width,
                height = (uint)imgData.Height,
                depth = 1
            };

            var rawImgData = imgData.CompressionType switch
            {
                UbiArtXbox360Texture.TextureCompressionType.DXT1 => DDSParser.DecompressDXT1(header, untiledImgData),
                UbiArtXbox360Texture.TextureCompressionType.DXT3 => DDSParser.DecompressDXT3(header, untiledImgData),
                UbiArtXbox360Texture.TextureCompressionType.DXT5 => DDSParser.DecompressDXT5(header, untiledImgData),
                _ => throw new ArgumentOutOfRangeException(nameof(imgData.CompressionType), imgData.CompressionType, null)
            };

            // Return the image
            return new MagickImage(rawImgData, new MagickReadSettings()
            {
                Format = MagickFormat.Rgba,
                Width = imgData.Width,
                Height = imgData.Height
            });
        }
    }
}