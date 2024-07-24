using ImageMagick;

namespace RayCarrot.RCP.Metro.Imaging;

public class TgaImageFormat : ImageMagickImageFormat
{
    public override string Name => "TGA";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".tga"),
    };

    public override MagickFormat Format => MagickFormat.Tga;
}