using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A GNF UbiArt texture file type (PS4)
/// </summary>
public class ArchiveFileType_GNFUbiArtTex : ArchiveFileType_BaseUbiArtTex
{
    /// <summary>
    /// The format
    /// </summary>
    protected override FileExtension Format => new(".gnf");

    /// <summary>
    /// The magic header for the format
    /// </summary>
    protected override uint? FormatMagic => 0x474E4620;

    /// <summary>
    /// Indicates if the format is fully supported and can be read as an image
    /// </summary>
    protected override bool IsFormatSupported => false;
}