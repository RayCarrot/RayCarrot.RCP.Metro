#nullable disable
using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Data for the <see cref="FilePatcher"/>
/// </summary>
public class FilePatcher_Patch
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="fileSize">The expected file size of the file to patch</param>
    /// <param name="patchEntries">The patch entries used to determine what data in the file to patch</param>
    public FilePatcher_Patch(uint fileSize, PatchEntry[] patchEntries)
    {
        FileSize = fileSize;
        PatchEntries = patchEntries ?? throw new ArgumentNullException(nameof(patchEntries));

        if (!PatchEntries.Any())
            throw new ArgumentException("At least one patch entry must be specified", nameof(patchEntries));
    }

    /// <summary>
    /// The expected file size of the file to patch
    /// </summary>
    public uint FileSize { get; }

    /// <summary>
    /// The patch entries used to determine what data in the file to patch
    /// </summary>
    public PatchEntry[] PatchEntries { get; }

    public record PatchEntry(long PatchOffset, byte[] OriginalBytes, byte[] PatchedBytes);
}