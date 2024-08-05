using ImageMagick;

namespace RayCarrot.RCP.Metro.Imaging;

public class PngImageFormat : ImageMagickImageFormat
{
    public override string Name => "PNG";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".png"),
    };

    public override MagickFormat Format => MagickFormat.Png;
}