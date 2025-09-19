namespace RayCarrot.RCP.Metro;

/// <summary>
/// Data for the <see cref="FilePatcher"/>
/// </summary>
public class FilePatcher_Patch
{
    /// <summary>
    /// Creates a new patch with a single possible file size
    /// </summary>
    /// <param name="fileSize">The expected file size of the file to patch</param>
    /// <param name="patchEntries">The patch entries used to determine what data in the file to patch</param>
    public FilePatcher_Patch(uint fileSize, PatchEntry[] patchEntries) : this(new[] { fileSize }, patchEntries) { }

    /// <summary>
    /// Creates a new patch with multiple possible file sizes
    /// </summary>
    /// <param name="fileSizes">The possible expected file sizes of the file to patch</param>
    /// <param name="patchEntries">The patch entries used to determine what data in the file to patch</param>
    public FilePatcher_Patch(uint[] fileSizes, PatchEntry[] patchEntries)
    {
        FileSizes = fileSizes;
        PatchEntries = patchEntries ?? throw new ArgumentNullException(nameof(patchEntries));

        if (!PatchEntries.Any())
            throw new ArgumentException("At least one patch entry must be specified", nameof(patchEntries));
    }

    /// <summary>
    /// The possible expected file sizes of the file to patch
    /// </summary>
    public uint[] FileSizes { get; }

    /// <summary>
    /// The patch entries used to determine what data in the file to patch
    /// </summary>
    public PatchEntry[] PatchEntries { get; }

    public record PatchEntry(long PatchOffset, byte[] OriginalBytes, PatchedBytesRevision[] PatchRevisions)
    {
        public PatchEntry(long PatchOffset, byte[] OriginalBytes, byte[] PatchedBytes) : 
            this(PatchOffset, OriginalBytes, new PatchedBytesRevision(0, PatchedBytes).YieldToArray()) { }
    }
    public record PatchedBytesRevision(int Version, byte[] Bytes);
}