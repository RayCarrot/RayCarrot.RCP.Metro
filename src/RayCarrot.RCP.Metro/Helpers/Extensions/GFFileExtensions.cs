using System.Drawing;
using System.Drawing.Imaging;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

// TODO-UPDATE: Replace this with a new ImageFormat class and rewrite to be more optimized
public static class GFFileExtensions
{
    /// <summary>
    /// Converts the .gf pixel data to raw bitmap data of a specified size
    /// </summary>
    /// <param name="gf">The GF file data</param>
    /// <param name="flipY">Indicates if the y-axis should be flipped</param>
    /// <returns>The raw image data</returns>
    public static RawBitmapData GetRawBitmapData(this GF gf, bool flipY = true)
    {
        // Get the format
        GF_Format format = gf.PixelFormat;
        PixelFormat pixelFormat = format.SupportsTransparency() ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;
        int bmpChannels = Image.GetPixelFormatSize(pixelFormat) / 8;
        int width = gf.Width;
        int height = gf.Height;

        // Create the pixel array
        byte[] rawPixelData = new byte[width * height * bmpChannels];

        // Enumerate each pixel
        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                // Get the offsets for the pixel colors
                long pixelOffset = (width * y + x) * gf.Channels;
                long rawOffset = (width * (flipY ? height - y - 1 : y) + x) * bmpChannels;

                // Get the pixels
                foreach (var b in gf.GetBGRAPixel(format, gf.PixelData, pixelOffset))
                {
                    rawPixelData[rawOffset] = b;
                    rawOffset++;
                }
            }
        }

        // Return the raw bitmap data
        return new RawBitmapData(width, height, rawPixelData, pixelFormat);
    }

    /// <summary>
    /// Imports a bitmap image into the file, keeping the structure based on the properties and generating mipmaps if needed. This will reset the mipmaps, requiring them to be generated again.
    /// </summary>
    /// <param name="gf">The GF file data</param>
    /// <param name="settings">The serializer settings</param>
    /// <param name="bmp">The bitmap data to import from</param>
    /// <param name="generateMipmaps">Indicates if mipmaps should be generated for the image</param>
    public static void ImportFromBitmap(this GF gf, OpenSpaceSettings settings, RawBitmapData bmp, bool generateMipmaps)
    {
        // Helper method for writing the pixel data
        void WritePixelData(RawBitmapData bitmapData, long offset)
        {
            // Make sure the pixel format is supported
            if (bitmapData.PixelFormat != PixelFormat.Format32bppArgb && bitmapData.PixelFormat != PixelFormat.Format24bppRgb)
                throw new Exception($"The bitmap pixel format {bitmapData.PixelFormat} is not supported for importing");

            // Get the number of bitmap channels
            int bmpChannels = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

            // Get the format
            GF_Format format = gf.PixelFormat;

            byte[] bmpColorData = new byte[4];

            for (uint y = 0; y < bitmapData.Height; y++)
            for (uint x = 0; x < bitmapData.Width; x++)
            {
                // Get the offsets for the pixel colors
                var pixelOffset = (bitmapData.Width * y + x) * gf.Channels + offset;

                // NOTE: We reverse the Y-axis here since the .gf images are always flipper vertically
                var rawOffset = (bitmapData.Width * (bitmapData.Height - y - 1) + x) * bmpChannels;

                // Get the bitmap color bytes for this pixel
                bmpColorData[0] = bitmapData.PixelData[rawOffset + 0];
                bmpColorData[1] = bitmapData.PixelData[rawOffset + 1];
                bmpColorData[2] = bitmapData.PixelData[rawOffset + 2];
                bmpColorData[3] = bmpChannels == 4 ? bitmapData.PixelData[rawOffset + 3] : (byte)255;

                // Get the pixels
                foreach (var b in gf.GetGfPixelBytes(format, bmpColorData))
                {
                    gf.PixelData[pixelOffset] = b;
                    pixelOffset++;
                }
            }
        }

        // Set size
        gf.Width = bmp.Width;
        gf.Height = bmp.Height;

        // Set the pixel count
        gf.PixelCount = gf.Width * gf.Height;

        // Update the mipmap count
        if (generateMipmaps && gf.SupportsMipmaps(settings))
            gf.MipmapsCount = gf.DeterminePreferredMipmapsCount();
        else
            gf.MipmapsCount = 0;

        // Enumerate each mipmap size
        foreach ((int Width, int Height) size in gf.GetExclusiveMipmapSizes())
        {
            // Get the mipmap pixel count
            var count = size.Width * size.Height;

            // Add to the total pixel count
            gf.PixelCount += count;
        }

        // Create the data array
        gf.PixelData = new byte[gf.Channels * gf.PixelCount];

        // Set the main pixel data
        WritePixelData(bmp, 0);

        // Keep track of the offset
        long mipmapOffset = gf.Width * gf.Height * gf.Channels;

        // Generate mipmaps if available
        if (gf.ExclusiveMipmapCount > 0)
        {
            // Get the bitmap
            using Bitmap bitmap = bmp.GetBitmap();

            // Generate every mipmap
            foreach ((int Width, int Height) size in gf.GetExclusiveMipmapSizes())
            {
                // Resize the bitmap
                using Bitmap resizedBmp = bitmap.ResizeImage(size.Width, size.Height, false);

                // Write the mipmap
                WritePixelData(new RawBitmapData(resizedBmp), mipmapOffset);

                // Increase the index
                mipmapOffset += size.Height * size.Width * gf.Channels;
            }
        }

        // Update the repeat byte
        gf.UpdateRepeatByte();
    }
}