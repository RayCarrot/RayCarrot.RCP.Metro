using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Bitmap"/>
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts a <see cref="Bitmap"/> to an <see cref="ImageSource"/>
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="disposeBitmap">Indicates if the bitmap image should be disposed</param>
        /// <returns></returns>
        public static ImageSource ToImageSource(this Bitmap bmp, bool disposeBitmap = true)
        {
            try
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                try
                {
                    var size = (rect.Width * rect.Height) * 4;

                    return BitmapSource.Create(bmp.Width, bmp.Height, bmp.HorizontalResolution, bmp.VerticalResolution, PixelFormats.Bgra32, null, bitmapData.Scan0, size, bitmapData.Stride);
                }
                finally
                {
                    bmp.UnlockBits(bitmapData);
                }
            }
            finally
            {
                if (disposeBitmap)
                    bmp.Dispose();
            }
        }
    }
}