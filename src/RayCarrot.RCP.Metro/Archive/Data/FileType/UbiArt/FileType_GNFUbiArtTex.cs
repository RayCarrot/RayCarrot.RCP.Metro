namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A GNF UbiArt texture file type (PS4)
/// </summary>
public class FileType_GNFUbiArtTex : FileType_BaseUbiArtTex
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