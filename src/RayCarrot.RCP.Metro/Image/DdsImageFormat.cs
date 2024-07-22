using ImageMagick;

namespace RayCarrot.RCP.Metro.Image;

// TODO-UPDATE: When encoding we want to handle alpha correctly for mipmaps and use box filter like
//              we did before. ImageMagick isn't good though, better to switch to something else.
public class DdsImageFormat : ImageMagickImageFormat
{
    public override string Name => "DDS";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".dds"),
    };

    public override MagickFormat Format => MagickFormat.Dds;
}