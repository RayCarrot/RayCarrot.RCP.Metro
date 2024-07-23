using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.Imaging;

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

    public byte[] Convert(RawImageDataPixelFormat newPixelFormat)
    {
        if (PixelFormat == newPixelFormat)
            return RawData;

        switch (PixelFormat)
        {
            case RawImageDataPixelFormat.Bgr24 when newPixelFormat is RawImageDataPixelFormat.Bgra32:
            {
                byte[] convertedData = new byte[Metadata.Width * Metadata.Height * 4];

                int originalIndex = 0;
                int convertedIndex = 0;

                for (int y = 0; y < Metadata.Height; y++)
                {
                    for (int x = 0; x < Metadata.Width; x++)
                    {
                        convertedData[convertedIndex + 0] = RawData[originalIndex + 0];
                        convertedData[convertedIndex + 1] = RawData[originalIndex + 1];
                        convertedData[convertedIndex + 2] = RawData[originalIndex + 2];
                        convertedData[convertedIndex + 3] = 0xFF;

                        originalIndex += 3;
                        convertedIndex += 4;
                    }
                }

                return convertedData;
            }

            case RawImageDataPixelFormat.Bgra32 when newPixelFormat is RawImageDataPixelFormat.Bgr24:
            {
                // Currently unused so no need to implement
                throw new NotImplementedException();
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public BitmapSource ToBitmapSource()
    {
        int stride = GetStride();
        PixelFormat format = GetWindowsPixelFormat();

        return BitmapSource.Create(Metadata.Width, Metadata.Height, 96, 96, format, null, RawData, stride);
    }
}