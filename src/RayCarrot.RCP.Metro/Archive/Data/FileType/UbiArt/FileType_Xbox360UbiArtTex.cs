using BinarySerializer;
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
        TextureCooked tex = manager.Context!.ReadStreamData<TextureCooked>(inputStream.Stream, name: inputStream.Name, mode: VirtualFileMode.DoNotClose, onPreSerialize: x =>
        {
            x.Pre_SerializeRawData = true;
            x.Pre_FileSize = inputStream.Stream.Length;
        });

        // Get the untiled image data
        byte[] untiledImgData = tex.Xbox360_D3DTexture.Untile(tex.RawData, true);

        DDSParser.DDSStruct header = new()
        {
            pixelformat = new DDSParser.DDSStruct.pixelformatstruct()
            {
                rgbbitcount = 32
            },
            width = (uint)tex.Xbox360_D3DTexture.ActualWidth,
            height = (uint)tex.Xbox360_D3DTexture.ActualHeight,
            depth = 1
        };

        byte[] rawImgData = tex.Xbox360_D3DTexture.DataFormat switch
        { 
            D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT1 => DDSParser.DecompressDXT1(header, untiledImgData),
            D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT2_3 => DDSParser.DecompressDXT3(header, untiledImgData),
            D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5 => DDSParser.DecompressDXT5(header, untiledImgData),
            _ => throw new ArgumentOutOfRangeException(nameof(tex.Xbox360_D3DTexture.DataFormat), tex.Xbox360_D3DTexture.DataFormat, null)
        };

        // TODO: Do this for other UbiArt platforms too?
        // Only remap if it's not using normal color mapping
        if (tex.Remap != 0x00010203)
        {
            byte[] colors = new byte[6];
            colors[4] = 0x00;
            colors[5] = 0xFF;

            for (int i = 0; i < rawImgData.Length; i += 4)
            {
                colors[0] = rawImgData[i + 3]; // Alpha
                colors[1] = rawImgData[i + 0]; // Red
                colors[2] = rawImgData[i + 1]; // Green
                colors[3] = rawImgData[i + 2]; // Blue

                rawImgData[i + 3] = colors[tex.Remap_A];
                rawImgData[i + 0] = colors[tex.Remap_R];
                rawImgData[i + 1] = colors[tex.Remap_G];
                rawImgData[i + 2] = colors[tex.Remap_B];
            }
        }

        // Return the image
        return new MagickImage(rawImgData, new MagickReadSettings()
        {
            Format = MagickFormat.Rgba,
            Width = tex.Xbox360_D3DTexture.ActualWidth,
            Height = tex.Xbox360_D3DTexture.ActualHeight
        });
    }
}