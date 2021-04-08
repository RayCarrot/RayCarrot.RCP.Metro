using ImageMagick;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A GXT UbiArt texture file type (PS Vita)
    /// </summary>
    public class ArchiveFileType_GXTUbiArtTex : ArchiveFileType_BaseUbiArtTex
    {
        /// <summary>
        /// The format
        /// </summary>
        protected override FileExtension Format => new FileExtension(".gxt");

        /// <summary>
        /// The magick format
        /// </summary>
        protected override MagickFormat MagickFormat => MagickFormat.Unknown;

        /// <summary>
        /// The magic header for the format
        /// </summary>
        protected override uint FormatMagic => 0x47585400;

        /// <summary>
        /// Indicates if the format is fully supported and can be read as an image
        /// </summary>
        protected override bool IsFormatSupported => false;
    }
}