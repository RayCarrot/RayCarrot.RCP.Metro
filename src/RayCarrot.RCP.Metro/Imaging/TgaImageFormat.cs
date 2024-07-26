using ImageMagick;

namespace RayCarrot.RCP.Metro.Imaging;

public class TgaImageFormat : ImageMagickImageFormat
{
    public TgaImageFormat(OrientationType orientation = OrientationType.TopLeft)
    {
        Orientation = orientation;
    }

    private OrientationType Orientation { get; }

    public override string Name => "TGA";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".tga"),
    };

    public override MagickFormat Format => MagickFormat.Tga;

    protected override void OnEncode(MagickImage img)
    {
        if (Orientation is OrientationType.BottomLeft or OrientationType.BottomRight)
            img.Flip();

        if (Orientation is OrientationType.TopRight or OrientationType.BottomRight)
            img.Flop();

        img.Orientation = Orientation;
    }
}