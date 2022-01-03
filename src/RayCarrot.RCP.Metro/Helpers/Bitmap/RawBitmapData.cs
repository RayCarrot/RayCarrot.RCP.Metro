using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Raw image data
/// </summary>
[Obsolete("Use BitmapSource instead")]
public class RawBitmapData
{
    /// <summary>
    /// Creates a new instance of <see cref="RawBitmapData"/> from a <see cref="Bitmap"/>
    /// </summary>
    /// <param name="bmp">The bitmap to get the raw data from</param>
    public RawBitmapData(Bitmap bmp)
    {
        using var bmpLock = new BitmapLock(bmp);

        Width = bmp.Width;
        Height = bmp.Height;
        PixelData = bmpLock.Pixels;
        PixelFormat = bmp.PixelFormat;
    }

    /// <summary>
    /// Creates a new instance of <see cref="RawBitmapData"/> from specified values
    /// </summary>
    /// <param name="width">The image width</param>
    /// <param name="height">The image height</param>
    /// <param name="pixelData">The pixel data</param>
    /// <param name="pixelFormat">The pixel format</param>
    public RawBitmapData(int width, int height, byte[] pixelData, PixelFormat pixelFormat)
    {
        Width = width;
        Height = height;
        PixelData = pixelData;
        PixelFormat = pixelFormat;
    }

    /// <summary>
    /// The image width
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The image height
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// The pixel data
    /// </summary>
    public byte[] PixelData { get; }

    /// <summary>
    /// The format of the pixel data
    /// </summary>
    public PixelFormat PixelFormat { get; }

    /// <summary>
    /// Gets a bitmap from the raw image data
    /// </summary>
    /// <returns>The bitmap</returns>
    public Bitmap GetBitmap()
    {
        // Create the bitmap
        Bitmap bmp = new Bitmap(Width, Height, PixelFormat);

        // Lock and update the pixels
        using (var bmpLock = new BitmapLock(bmp))
            bmpLock.Pixels = PixelData;

        // Return the bitmap
        return bmp;
    }
}