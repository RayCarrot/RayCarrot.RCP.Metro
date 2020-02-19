using System;
using System.Drawing.Imaging;
using ImageMagick;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Image related helper methods
    /// </summary>
    public static class ImageHelpers
    {
        /// <summary>
        /// Gets the commonly supported bitmap file extensions
        /// </summary>
        /// <returns>The commonly supported bitmap file extensions</returns>
        public static string[] GetSupportedBitmapExtensions()
        {
            return new string[]
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".bmp",
            };
        }

        /// <summary>
        /// Gets the commonly supported Magick file extensions
        /// </summary>
        /// <returns>The commonly supported Magick file extensions</returns>
        public static string[] GetSupportedMagickExtensions()
        {
            return new string[]
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".bmp",
            };
        }

        /// <summary>
        /// Gets the <see cref="ImageFormat"/> from the specified file extension
        /// </summary>
        /// <param name="fileExtension">The file extension</param>
        /// <returns>The image format</returns>
        public static ImageFormat GetImageFormat(FileExtension fileExtension)
        {
            // Get the format
            return fileExtension.PrimaryFileExtension switch
            {
                ".png" => ImageFormat.Png,
                ".jpeg" => ImageFormat.Jpeg,
                ".jpg" => ImageFormat.Jpeg,
                ".bmp" => ImageFormat.Bmp,
                _ => throw new Exception($"The specified file format {fileExtension} is not supported")
            };
        }

        /// <summary>
        /// Gets the <see cref="MagickFormat"/> from the specified file extension
        /// </summary>
        /// <param name="fileExtension">The file extension</param>
        /// <returns>The Magick format</returns>
        public static MagickFormat GetMagickFormat(FileExtension fileExtension)
        {
            // Get the format
            return fileExtension.PrimaryFileExtension switch
            {
                ".dds" => MagickFormat.Dds,
                ".png" => MagickFormat.Png,
                ".jpeg" => MagickFormat.Jpeg,
                ".jpg" => MagickFormat.Jpeg,
                ".bmp" => MagickFormat.Bmp,
                _ => throw new Exception($"The specified file format {fileExtension} is not supported")
            };
        }
    }
}