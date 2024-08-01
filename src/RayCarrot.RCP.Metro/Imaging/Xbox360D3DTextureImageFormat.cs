using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.Imaging;

public class Xbox360D3DTextureImageFormat : ImageFormat
{
    public override string Id => "Xbox360_D3DTexture";
    public override string Name => "D3DTexture";

    public override bool CanDecode => true;
    public override bool CanEncode => false;

    private ImageMetadata GetMetadata(D3DTexture tex)
    {
        return new ImageMetadata(tex.ActualWidth, tex.ActualHeight)
        {
            MipmapsCount = tex.MaxMipLevel,
            Encoding = tex.DataFormat.ToString(),
        };
    }

    public override ImageMetadata GetMetadata(Stream inputStream)
    {
        using Context context = new RCPContext(String.Empty);
        D3DTexture header = context.ReadStreamData<D3DTexture>(inputStream, mode: VirtualFileMode.DoNotClose);
        return GetMetadata(header);
    }

    public override RawImageData Decode(Stream inputStream)
    {
        // Read the header
        using Context context = new RCPContext(String.Empty);
        D3DTexture header = context.ReadStreamData<D3DTexture>(inputStream, endian: Endian.Big, mode: VirtualFileMode.DoNotClose, maintainPosition: true);
        bool isCompressed = header.DataFormat != D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_8_8_8_8;

        // TODO: Can we determine the length from the header instead?
        // Read the raw image data
        byte[] imgData = new byte[inputStream.Length - inputStream.Position];
        inputStream.Read(imgData, 0, imgData.Length);

        // Untiled the image data
        imgData = header.Untile(imgData, swapBytes: isCompressed);

        // TODO-UPDATE: Convert to a DDS file and use DDS format to decode. That way we can also keep the mipmaps.
        // Decompress
        if (isCompressed)
        {
            DDSParser.DDSStruct ddsHeader = new()
            {
                pixelformat = new DDSParser.DDSStruct.pixelformatstruct() { rgbbitcount = 32 },
                width = (uint)header.ActualWidth,
                height = (uint)header.ActualHeight,
                depth = 1
            };

            imgData = header.DataFormat switch
            {
                D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT1 => DDSParser.DecompressDXT1(ddsHeader, imgData),
                D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT2_3 => DDSParser.DecompressDXT3(ddsHeader, imgData),
                D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_DXT4_5 => DDSParser.DecompressDXT5(ddsHeader, imgData),
                _ => throw new ArgumentOutOfRangeException(nameof(header.DataFormat), header.DataFormat, null)
            };
        }

        // Remove mipmaps
        Array.Resize(ref imgData, header.ActualWidth * header.ActualHeight * 4);

        RawImageDataPixelFormat pixelFormat;

        if (header.DataFormat == D3DTexture.GPUTEXTUREFORMAT.GPUTEXTUREFORMAT_8_8_8_8)
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

        return new RawImageData(imgData, pixelFormat, GetMetadata(header));
    }

    public override ImageMetadata Encode(RawImageData data, Stream outputStream)
    {
        throw new InvalidOperationException();
    }
}