﻿namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A GXT UbiArt texture file type (PS Vita)
/// </summary>
public class FileType_GXTUbiArtTex : FileType_BaseUbiArtTex
{
    /// <summary>
    /// The format
    /// </summary>
    protected override FileExtension Format => new(".gxt");

    /// <summary>
    /// The magic header for the format
    /// </summary>
    protected override uint? FormatMagic => 0x47585400;

    /// <summary>
    /// Indicates if the format is fully supported and can be read as an image
    /// </summary>
    protected override bool IsFormatSupported => false;
}