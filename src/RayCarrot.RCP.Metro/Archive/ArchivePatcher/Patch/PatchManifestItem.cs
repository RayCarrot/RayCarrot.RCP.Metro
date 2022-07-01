using System;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifestItem
{
    public PatchManifestItem(string id, int containerVersion)
    {
        ID = id;
        ContainerVersion = containerVersion;
        ModifiedDate = DateTime.Now;
    }

    public string ID { get; }
    public string? ThumbnailFileName { get; set; } // TODO: Replace with bool - keep file name constant
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public PatchFlags Flags { get; set; }
    public long TotalSize { get; set; }
    public DateTime ModifiedDate { get; }
    public int Revision { get; set; }
    public string[]? AddedFiles { get; set; }
    public string[]? AddedFileChecksums { get; set; }
    public string[]? RemovedFiles { get; set; }
    public int ContainerVersion { get; }
    public bool IsEnabled { get; set; }
}