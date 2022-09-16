using System;
using BinarySerializer.UbiArt;
using ImageMagick;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A Xbox 360 UbiArt texture file type (Xbox 360)
/// </summary>
public class FileType_Xbox360UbiArtTex : FileType_BaseUbiArtTex
{
    /// <summary>
    /// Indicates if the format is fully supported and can be read as an image
    /// </summary>
    protected override bool IsFormatSupported => true;

    /// <summary>
    /// The supported formats to import from
    /// </summary>
    public override FileExtension[] ImportFormats => Array.Empty<FileExtension>();

    /// <summary>
    /// The format
    /// </summary>
    protected override FileExtension Format => new FileExtension(".dds_xbox");

    public override bool IsOfType(ArchiveFileStream inputStream, IArchiveDataManager manager, TextureCooked? tex)
    {
        UbiArtSettings settings = manager.Context!.GetRequiredSettings<UbiArtSettings>();

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
    protected override MagickImage GetImage(ArchiveFileStream inputStream, FileExtension format, IArchiveDataManager manager)
    {
        // Serialize data
        TextureCooked tex = manager.Context!.ReadStreamData<TextureCooked>(inputStream.Stream, name: inputStream.Name, leaveOpen: true, onPreSerialize: x =>
        {
            x.Pre_SerializeImageData = true;
            x.Pre_FileSize = inputStream.Stream.Length;
        });

        // Get the untiled image data
        byte[] untiledImgData = tex.Header_Xbox360.Untile(tex.ImageData, true);

        DDSParser.DDSStruct header = new()
        {
            pixelformat = new DDSParser.DDSStruct.pixelformatstruct()
            {
                rgbbitcount = 32
            },
            width = (uint)tex.Header_Xbox360.Width,
            height = (uint)tex.Header_Xbox360.Height,
            depth = 1
        };

        byte[] rawImgData = tex.Header_Xbox360.CompressionType switch
        {
            TextureCooked_Xbox360Header.TextureCompressionType.DXT1 => DDSParser.DecompressDXT1(header, untiledImgData),
            TextureCooked_Xbox360Header.TextureCompressionType.DXT3 => DDSParser.DecompressDXT3(header, untiledImgData),
            TextureCooked_Xbox360Header.TextureCompressionType.DXT5 => DDSParser.DecompressDXT5(header, untiledImgData),
            _ => throw new ArgumentOutOfRangeException(nameof(tex.Header_Xbox360.CompressionType), tex.Header_Xbox360.CompressionType, null)
        };

        // Return the image
        return new MagickImage(rawImgData, new MagickReadSettings()
        {
            Format = MagickFormat.Rgba,
            Width = tex.Header_Xbox360.Width,
            Height = tex.Header_Xbox360.Height
        });
    }
}