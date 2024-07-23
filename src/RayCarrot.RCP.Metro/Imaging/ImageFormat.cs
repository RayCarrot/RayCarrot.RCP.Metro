using System.IO;

namespace RayCarrot.RCP.Metro.Imaging;

public abstract class ImageFormat
{
    public abstract string Name { get; }

    public abstract bool CanDecode { get; }
    public abstract bool CanEncode { get; }

    public virtual FileExtension[] FileExtensions => Array.Empty<FileExtension>();

    public abstract ImageMetadata GetMetadata(Stream inputStream);

    public abstract RawImageData Decode(Stream inputStream);
    public abstract ImageMetadata Encode(RawImageData data, Stream outputStream);

    public virtual ImageMetadata Convert(Stream inputStream, Stream outputStream, ImageFormat outputFormat)
    {
        RawImageData decodedData = Decode(inputStream);
        return outputFormat.Encode(decodedData, outputStream);
    }
}