using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A bitmap wrapper for locking the pixels for faster accessing
/// </summary>
public class BitmapLock : IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="bmp">The bitmap</param>
    public BitmapLock(Bitmap bmp)
    {
        // Get the bitmap
        SourceBmp = bmp;

        // Get width and height of bitmap
        Width = SourceBmp.Width;
        Height = SourceBmp.Height;

        // Create rectangle to lock
        Rectangle rect = new Rectangle(0, 0, Width, Height);

        // Make sure the pixel format is supported
        if (!SupportedPixelFormats.Contains(PixelFormat))
            throw new ArgumentException($"The pixel format {PixelFormat} of the bitmap is not supported");

        // Lock bitmap and return bitmap data
        BitmapData = SourceBmp.LockBits(rect, ImageLockMode.ReadWrite, SourceBmp.PixelFormat);

        // Create byte array to copy pixel values
        Pixels = new byte[Width * Height * (System.Drawing.Image.GetPixelFormatSize(PixelFormat) / 8)];

        // Get the pointer address
        Iptr = BitmapData.Scan0;

        // Copy data from pointer to array
        Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
    }

    #endregion

    #region Protected Properties

    /// <summary>
    /// The source bitmap
    /// </summary>
    protected Bitmap SourceBmp { get; }
        
    /// <summary>
    /// The pointer address for the pixels
    /// </summary>
    protected IntPtr Iptr { get; }

    /// <summary>
    /// The bitmap data
    /// </summary>
    protected BitmapData BitmapData { get; }

    /// <summary>
    /// The pixel format of the bitmap
    /// </summary>
    protected PixelFormat PixelFormat => SourceBmp.PixelFormat;

    /// <summary>
    /// The currently supported pixel formats
    /// </summary>
    protected PixelFormat[] SupportedPixelFormats => new[]
    {
        PixelFormat.Format32bppArgb,
        PixelFormat.Format24bppRgb
    };

    /// <summary>
    /// The bitmap width
    /// </summary>
    protected int Width { get; }
        
    /// <summary>
    /// The bitmap height
    /// </summary>
    protected int Height { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The pixel array
    /// </summary>
    public byte[] Pixels { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the color of the specified pixel
    /// </summary>
    /// <param name="x">The x position</param>
    /// <param name="y">The y position</param>
    /// <returns>The color</returns>
    public Color GetPixel(int x, int y)
    {
        // Get color components count
        int cCount = System.Drawing.Image.GetPixelFormatSize(PixelFormat) / 8;

        // Get start index of the specified pixel
        int i = ((y * Width) + x) * cCount;

        if (i > Pixels.Length - cCount)
            throw new IndexOutOfRangeException();

        byte b;
        byte g;
        byte r;
        byte a;

        // Get the pixel
        switch (PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                b = Pixels[i + 0];
                g = Pixels[i + 1];
                r = Pixels[i + 2];

                return Color.FromArgb(r, g, b);

            case PixelFormat.Format32bppArgb:

                b = Pixels[i + 0];
                g = Pixels[i + 1];
                r = Pixels[i + 2];
                a = Pixels[i + 3];

                return Color.FromArgb(a, r, g, b);

            default:
                throw new ArgumentException($"The pixel format {PixelFormat} of the bitmap is not supported");
        }
    }

    /// <summary>
    /// Sets the color of the specified pixel
    /// </summary>
    /// <param name="x">The x position</param>
    /// <param name="y">The y position</param>
    /// <param name="color">The color</param>
    public void SetPixel(int x, int y, Color color)
    {
        // Get color components count
        int cCount = System.Drawing.Image.GetPixelFormatSize(PixelFormat) / 8;

        // Get start index of the specified pixel
        int i = ((y * Width) + x) * cCount;

        // Set the pixel
        switch (PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                Pixels[i + 0] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;

                break;

            case PixelFormat.Format32bppArgb:

                Pixels[i + 0] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;

                break;

            default:
                throw new ArgumentException($"The pixel format {PixelFormat} of the bitmap is not supported");
        }
    }

    /// <summary>
    /// Checks if the bitmap utilizes the alpha channel
    /// </summary>
    /// <returns>True if it is utilized, otherwise false</returns>
    public bool UtilizesAlpha()
    {
        if (PixelFormat == PixelFormat.Format32bppArgb)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Pixels[(((y * Width) + x) * 4) + 3] != Byte.MaxValue)
                        return true;
                }
            }

            return false;
        }
        else if (PixelFormat == PixelFormat.Format24bppRgb)
        {
            return false;
        }
        else
        {
            throw new ArgumentException($"The pixel format {PixelFormat} of the bitmap is not supported");
        }
    }

    /// <summary>
    /// Unlocks the bits of the bitmap
    /// </summary>
    public void Dispose()
    {
        // Copy data from byte array to pointer
        Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

        // Unlock bitmap data
        SourceBmp.UnlockBits(BitmapData);
    }

    #endregion
}