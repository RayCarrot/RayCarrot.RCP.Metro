using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro.Archive;

// Files added to the archive. No data is saved for these. Restore by deleting them.
// Files replaced in the archive. The original file is saved. Restore by replacing it back.
// Files removed from the archive. The original file is saved. Restore by adding it back.
public record PatchHistoryManifest
{
    public PatchHistoryManifest(string id, int containerVersion)
    {
        ID = id;
        ContainerVersion = containerVersion;
        ModifiedDate = DateTime.Now;

        AddedFiles = new List<string>();
        AddedFileChecksums = new List<string>();
        ReplacedFiles = new List<string>();
        ReplacedFileChecksums = new List<string>();
        RemovedFiles = new List<string>();
    }

    public string ID { get; }
    public long TotalSize { get; set; }
    public DateTime ModifiedDate { get; }
    public List<string> AddedFiles { get; }
    public List<string> AddedFileChecksums { get; }
    public List<string> ReplacedFiles { get; }
    public List<string> ReplacedFileChecksums { get; }
    public List<string> RemovedFiles { get; }
    public int ContainerVersion { get; }
}