using System;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifestItem(string ID, int ContainerVersion)
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public PatchFlags Flags { get; set; }
    public long TotalSize { get; set; }
    public DateTime ModifiedDate { get; } = DateTime.Now;
    public int Revision { get; set; }
    public string[]? AddedFiles { get; set; }
    public string[]? AddedFileChecksums { get; set; }
    public string[]? RemovedFiles { get; set; }
    public bool IsEnabled { get; set; }
}