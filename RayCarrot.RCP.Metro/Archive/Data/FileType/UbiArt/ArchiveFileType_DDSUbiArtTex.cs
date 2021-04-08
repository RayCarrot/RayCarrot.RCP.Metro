using ImageMagick;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A DDS UbiArt texture file type
    /// </summary>
    public class ArchiveFileType_DDSUbiArtTex : ArchiveFileType_BaseUbiArtTex
    {
        /// <summary>
        /// The format
        /// </summary>
        protected override FileExtension Format => new FileExtension(".dds");

        /// <summary>
        /// The magick format
        /// </summary>
        protected override MagickFormat MagickFormat => MagickFormat.Dds;

        /// <summary>
        /// The magic header for the format
        /// </summary>
        protected override uint FormatMagic => 0x44445320;

        /// <summary>
        /// Indicates if the format is fully supported and can be read as an image
        /// </summary>
        protected override bool IsFormatSupported => true;
    }
}