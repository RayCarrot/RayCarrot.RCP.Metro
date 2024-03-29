﻿namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A GTX UbiArt texture file type (Wii U)
/// </summary>
public class FileType_GTXUbiArtTex : FileType_BaseUbiArtTex
{
    /// <summary>
    /// The format
    /// </summary>
    protected override FileExtension Format => new(".gtx");

    /// <summary>
    /// The magic header for the format
    /// </summary>
    protected override uint? FormatMagic => 0x47667832;

    /// <summary>
    /// Indicates if the format is fully supported and can be read as an image
    /// </summary>
    protected override bool IsFormatSupported => false;
}