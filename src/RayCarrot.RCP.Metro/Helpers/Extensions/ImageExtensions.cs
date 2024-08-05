using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Image"/>
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Resizes the image using high quality bilinear interpolation
    /// </summary>
    /// <param name="image">The image to resize</param>
    /// <param name="width">The width to resize to</param>
    /// <param name="height">The height to resize to</param>
    /// <returns>The resized image</returns>
    public static Bitmap Resize(this Image image, int width, int height)
    {
        if (image == null) 
            throw new ArgumentNullException(nameof(image));

        // Create the frame
        Rectangle destRect = new(0, 0, width, height);

        // Crete the image to output
        Bitmap destImage = new(width, height);

        // Set the resolution
        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        // Create a graphic
        using Graphics graphics = Graphics.FromImage(destImage);

        // Set properties
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using ImageAttributes attr = new();

        attr.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);

        return destImage;
    }
}