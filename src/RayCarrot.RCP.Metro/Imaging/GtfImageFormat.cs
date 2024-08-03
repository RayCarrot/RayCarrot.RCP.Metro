﻿using System.IO;
using BinarySerializer;
using BinarySerializer.PlayStation.PS3;

namespace RayCarrot.RCP.Metro.Imaging;

public class GtfImageFormat : ImageFormat
{
    public override string Id => "GTF";
    public override string Name => "GTF";

    public override bool CanDecode => true;
    public override bool CanEncode => false;

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".gtf"),
    };

    private ImageMetadata GetMetadata(GTFTexture tex)
    {
        return new ImageMetadata(tex.Width, tex.Height)
        {
            MipmapsCount = tex.MipmapLevels,
            Encoding = tex.Format.ToString(),
        };
    }

    public override ImageMetadata GetMetadata(Stream inputStream)
    {
        // Read the file
        using Context context = new RCPContext(String.Empty);
        GTF gtf = context.ReadStreamData<GTF>(inputStream, endian: Endian.Big, mode: VirtualFileMode.DoNotClose, maintainPosition: true);

        return GetMetadata(gtf.Textures[0]);
    }

    public override RawImageData Decode(Stream inputStream)
    {
        // Read the file
        using Context context = new RCPContext(String.Empty);
        GTF gtf = context.ReadStreamData<GTF>(inputStream, endian: Endian.Big, mode: VirtualFileMode.DoNotClose, maintainPosition: true);

        if (gtf.TexturesCount != 1)
            throw new InvalidOperationException("GTF files with more than 1 texture are not supported");

        GTFTexture texture = gtf.Textures[0];
        byte[] imgData = texture.TextureData;

        if (texture.Cubemap || texture.Dimension != GTFDimension.Dimension2)
            throw new InvalidOperationException("Only 2D GTF textures are supported");

        // TODO-UPDATE: Convert to a DDS file and use DDS format to decode. That way we can also keep the mipmaps.
        switch (texture.Format)
        {
            case GTFFormat.A8R8G8B8:
                // TODO-UPDATE: Unswizzle
                break;

            // Decompress
            case GTFFormat.COMPRESSED_DXT1:
            case GTFFormat.COMPRESSED_DXT23:
            case GTFFormat.COMPRESSED_DXT45:
                DDSParser.DDSStruct ddsHeader = new()
                {
                    pixelformat = new DDSParser.DDSStruct.pixelformatstruct() { rgbbitcount = 32 },
                    width = texture.Width,
                    height = texture.Height,
                    depth = texture.Depth
                };

                imgData = texture.Format switch
                {
                    GTFFormat.COMPRESSED_DXT1 => DDSParser.DecompressDXT1(ddsHeader, imgData),
                    GTFFormat.COMPRESSED_DXT23 => DDSParser.DecompressDXT3(ddsHeader, imgData),
                    GTFFormat.COMPRESSED_DXT45 => DDSParser.DecompressDXT5(ddsHeader, imgData),
                    _ => throw new ArgumentOutOfRangeException(nameof(texture.Format), texture.Format, null)
                };
                break;

            default:
                throw new InvalidOperationException($"The GTF format {texture.Format} is not supported");
        }

        // Remove mipmaps
        Array.Resize(ref imgData, texture.Width * texture.Height * 4);

        RawImageDataPixelFormat pixelFormat;

        if (texture.Format == GTFFormat.A8R8G8B8)
        {
            // Convert ARGB to BGRA
            pixelFormat = RawImageDataPixelFormat.Bgra32;
            for (int i = 0; i < imgData.Length; i += 4)
            {
                byte a = imgData[i + 0];
                byte r = imgData[i + 1];
                byte g = imgData[i + 2];
                byte b = imgData[i + 3];

                imgData[i + 0] = b;
                imgData[i + 1] = g;
                imgData[i + 2] = r;
                imgData[i + 3] = a;
            }
        }
        else
        {
            // Convert RGBA to BGRA
            pixelFormat = RawImageDataPixelFormat.Bgra32;
            for (int i = 0; i < imgData.Length; i += 4)
            {
                byte r = imgData[i + 0];
                byte g = imgData[i + 1];
                byte b = imgData[i + 2];
                byte a = imgData[i + 3];

                imgData[i + 0] = b;
                imgData[i + 1] = g;
                imgData[i + 2] = r;
                imgData[i + 3] = a;
            }
        }

        return new RawImageData(imgData, pixelFormat, GetMetadata(texture));
    }

    public override ImageMetadata Encode(RawImageData data, Stream outputStream)
    {
        throw new InvalidOperationException();
    }
}