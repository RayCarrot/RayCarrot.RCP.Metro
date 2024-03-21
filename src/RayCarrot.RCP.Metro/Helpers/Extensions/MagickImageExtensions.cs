using ImageMagick;

namespace RayCarrot.RCP.Metro;

public static class MagickImageExtensions
{
    /// <summary>
    /// Manually creates mipmaps for a DDS image where the alpha is not blended. This fixes issues
    /// where color information is lost if a pixel is fully transparent.
    /// </summary>
    /// <param name="img">The original image</param>
    /// <param name="compress">Indicates if the image should be compressed</param>
    /// <param name="filterType">The filter type to use when resizing</param>
    /// <returns>The image collection with the mipmaps</returns>
    public static MagickImageCollection CreateDdsWithMipmaps(this MagickImage img, bool compress, FilterType filterType = FilterType.Triangle)
    {
        // Create a collection for the mipmaps
        MagickImageCollection collection = new();
        img.Format = MagickFormat.Dds;

        // Set defines
        if (!compress)
            img.Settings.SetDefine(MagickFormat.Dds, "compression", "none");
        img.Settings.SetDefine(MagickFormat.Dds, "mipmaps", "fromlist");

        // Add full image
        collection.Add(img);

        // Create images for rgb and alpha
        using MagickImage color = new(img);
        using MagickImage alpha = new(img);
        color.Alpha(AlphaOption.Off);
        alpha.Alpha(AlphaOption.Extract);

        int w = img.Width;
        int h = img.Height;

        // Create mipmaps
        divide();
        while (w != 1 || h != 1)
        {
            MagickImage mip = new(color);
            MagickImage mipAlpha = new(alpha);

            mip.FilterType = filterType;
            mipAlpha.FilterType = filterType;

            mip.Resize(w, h);
            mipAlpha.Resize(w, h);

            mip.Composite(mipAlpha, CompositeOperator.CopyAlpha);
            collection.Add(mip);

            divide();
        }

        return collection;

        void divide()
        {
            if (w > 1)
                w >>= 1;

            if (h > 1)
                h >>= 1;
        }
    }
}