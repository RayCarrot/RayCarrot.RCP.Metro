using System.IO;

namespace RayCarrot.RCP.Metro.Imaging;

public class PvrImageFormat : ImageFormat
{
    public override string Id => "PVR";
    public override string Name => "PVR";

    public override bool CanDecode => false;
    public override bool CanEncode => false;

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".pvr"),
    };

    public override ImageMetadata GetMetadata(Stream inputStream)
    {
        throw new InvalidOperationException();
    }

    public override RawImageData Decode(Stream inputStream)
    {
        throw new InvalidOperationException();
    }

    public override ImageMetadata Encode(RawImageData data, Stream outputStream)
    {
        throw new InvalidOperationException();
    }
}