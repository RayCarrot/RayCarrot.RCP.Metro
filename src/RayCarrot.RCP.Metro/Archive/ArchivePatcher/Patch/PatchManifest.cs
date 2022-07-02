using System;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifest
{
    public PatchManifest(string id, int containerVersion)
    {
        ID = id;
        ContainerVersion = containerVersion;
    }

    public string ID { get; }
    public int ContainerVersion { get; }
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
}