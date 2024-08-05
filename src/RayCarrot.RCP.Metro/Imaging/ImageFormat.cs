using System.IO;

namespace RayCarrot.RCP.Metro.Imaging;

public abstract class ImageFormat : IEquatable<ImageFormat>
{
    #region Public Properties

    public abstract string Id { get; }

    public abstract string Name { get; }

    public abstract bool CanDecode { get; }
    public abstract bool CanEncode { get; }

    public virtual FileExtension[] FileExtensions => Array.Empty<FileExtension>();

    #endregion

    #region Public Methods

    public abstract ImageMetadata GetMetadata(Stream inputStream);

    public abstract RawImageData Decode(Stream inputStream);
    public abstract ImageMetadata Encode(RawImageData data, Stream outputStream);

    public virtual ImageMetadata Convert(Stream inputStream, Stream outputStream, ImageFormat outputFormat)
    {
        RawImageData decodedData = Decode(inputStream);
        return outputFormat.Encode(decodedData, outputStream);
    }

    #endregion

    #region Equality

    public bool Equals(ImageFormat? other) => Id == other?.Id;
    public override bool Equals(object? obj) => obj is ImageFormat imgFormat && Equals(imgFormat);
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(ImageFormat? left, ImageFormat? right) => Equals(left, right);
    public static bool operator !=(ImageFormat? left, ImageFormat? right) => !Equals(left, right);

    #endregion
}