using ImageMagick;

namespace RayCarrot.RCP.Metro.Imaging;

public class PcxImageFormat : ImageMagickImageFormat
{
    public override string Name => "PCX";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".pcx"),
    };

    public override MagickFormat Format => MagickFormat.Pcx;
}