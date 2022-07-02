using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchHistoryManifest
{
    public PatchHistoryManifest(string id, int containerVersion)
    {
        ID = id;
        ContainerVersion = containerVersion;
    }

    public string ID { get; }
    public int ContainerVersion { get; }

    public long TotalSize { get; set; }
    public DateTime ModifiedDate { get; } = DateTime.Now;

    // Files added to the archive. No data is saved for these. Restore by deleting them.
    public List<string> AddedFiles { get; } = new();
    public List<string> AddedFileChecksums { get; } = new();
    
    // Files replaced in the archive. The original file is saved. Restore by replacing it back.
    public List<string> ReplacedFiles { get; } = new();
    public List<string> ReplacedFileChecksums { get; } = new();
    
    // Files removed from the archive. The original file is saved. Restore by adding it back.
    public List<string> RemovedFiles { get; } = new();
}