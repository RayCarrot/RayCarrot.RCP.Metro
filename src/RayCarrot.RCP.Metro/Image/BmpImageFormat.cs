﻿using ImageMagick;

namespace RayCarrot.RCP.Metro.Image;

public class BmpImageFormat : ImageMagickImageFormat
{
    public override string Name => "BMP";

    public override FileExtension[] FileExtensions { get; } =
    {
        new(".bmp"),
    };

    public override MagickFormat Format => MagickFormat.Bmp;
}