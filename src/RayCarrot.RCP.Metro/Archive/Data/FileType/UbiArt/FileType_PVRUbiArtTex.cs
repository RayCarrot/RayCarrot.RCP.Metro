namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A PVR UbiArt texture file type (iOS)
/// </summary>
public class FileType_PVRUbiArtTex : FileType_BaseUbiArtTex
{
    /// <summary>
    /// The format
    /// </summary>
    protected override FileExtension Format => new(".pvr");

    /// <summary>
    /// The magic header for the format
    /// </summary>
    protected override uint? FormatMagic => 0x50565203;

    /// <summary>
    /// Indicates if the format is fully supported and can be read as an image
    /// </summary>
    protected override bool IsFormatSupported => false;
}