using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.Image;

public class RawImageData
{
    public RawImageData(byte[] rawData, RawImageDataPixelFormat pixelFormat, ImageMetadata metadata)
    {
        RawData = rawData;
        PixelFormat = pixelFormat;
        Metadata = metadata;
    }

    public byte[] RawData { get; }
    public RawImageDataPixelFormat PixelFormat { get; }
    public ImageMetadata Metadata { get; }

    public int GetBitsPerPixel()
    {
        return PixelFormat switch
        {
            RawImageDataPixelFormat.Bgr24 => 24,
            RawImageDataPixelFormat.Bgra32 => 32,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public int GetStride()
    {
        int bpp = GetBitsPerPixel();
        int step = bpp / 8;
        return Metadata.Width * step;
    }

    public PixelFormat GetWindowsPixelFormat()
    {
        return PixelFormat switch
        {
            RawImageDataPixelFormat.Bgr24 => PixelFormats.Bgr24,
            RawImageDataPixelFormat.Bgra32 => PixelFormats.Bgra32,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public BitmapSource ToBitmapSource()
    {
        int stride = GetStride();
        PixelFormat format = GetWindowsPixelFormat();

        return BitmapSource.Create(Metadata.Width, Metadata.Height, 96, 96, format, null, RawData, stride);
    }
}