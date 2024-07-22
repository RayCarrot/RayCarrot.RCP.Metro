using ImageMagick;

namespace RayCarrot.RCP.Metro.Image;

public class JpgImageFormat : ImageMagickImageFormat
{
    public override string Name => "JPG";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".jpg"),
        new(".jpeg"),
    };

    public override MagickFormat Format => MagickFormat.Jpg;
}