using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="ShellThumbnail"/>
/// </summary>
public static class ShellThumbnailExtensions
{
    /// <summary>
    /// Gets a transparent bitmap icon or thumbnail from a <see cref="ShellThumbnail"/> based on size
    /// </summary>
    /// <param name="shellThumbnail">The shell thumbnail to get the bitmap from</param>
    /// <param name="shellThumbnailSize">The size of the thumbnail or icon to retrieve</param>
    /// <returns>The thumbnail or icon</returns>
    public static Bitmap GetTransparentBitmap(this ShellThumbnail shellThumbnail, ShellThumbnailSize shellThumbnailSize = ShellThumbnailSize.Default)
    {
        return shellThumbnailSize switch
        {
            ShellThumbnailSize.Default => CreateAlphaBitmap(shellThumbnail.Bitmap, PixelFormat.Format32bppArgb),
            ShellThumbnailSize.Small => CreateAlphaBitmap(shellThumbnail.SmallBitmap, PixelFormat.Format32bppArgb),
            ShellThumbnailSize.Medium => CreateAlphaBitmap(shellThumbnail.MediumBitmap, PixelFormat.Format32bppArgb),
            ShellThumbnailSize.Large => CreateAlphaBitmap(shellThumbnail.LargeBitmap, PixelFormat.Format32bppArgb),
            ShellThumbnailSize.ExtraLarge => CreateAlphaBitmap(shellThumbnail.ExtraLargeBitmap, PixelFormat.Format32bppArgb),
            _ => CreateAlphaBitmap(shellThumbnail.Bitmap, PixelFormat.Format32bppArgb)
        };
    }

    private static Bitmap CreateAlphaBitmap(Bitmap srcBitmap, PixelFormat targetPixelFormat)
    {
        if (Image.GetPixelFormatSize(srcBitmap.PixelFormat) < 32) 
            return srcBitmap;

        Bitmap result = new Bitmap(srcBitmap.Width, srcBitmap.Height, targetPixelFormat);

        Rectangle bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);

        BitmapData srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);

        bool isAlplaBitmap = false;

        try
        {
            for (int y = 0; y <= srcData.Height - 1; y++)
            {
                for (int x = 0; x <= srcData.Width - 1; x++)
                {
                    Color pixelColor = Color.FromArgb(Marshal.ReadInt32(srcData.Scan0, (srcData.Stride * y) + (4 * x)));

                    if (!isAlplaBitmap && (pixelColor.A > 0 & pixelColor.A < 255)) 
                        isAlplaBitmap = true;

                    result.SetPixel(x, y, pixelColor);
                }
            }
        }
        finally
        {
            srcBitmap.UnlockBits(srcData);
        }

        return isAlplaBitmap ? result : srcBitmap;
    }
}