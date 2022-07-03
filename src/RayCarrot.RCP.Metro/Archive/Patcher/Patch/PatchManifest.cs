using System;
using System.Linq;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifest
{
    public PatchManifest(string id, int containerVersion, string? name, string? description, string? author, PatchFlags flags, long totalSize, DateTime modifiedDate, int revision, string[]? addedFiles, string[]? addedFileChecksums, string[]? removedFiles, string[]? assets)
    {
        ID = id;
        ContainerVersion = containerVersion;
        Name = name;
        Description = description;
        Author = author;
        Flags = flags;
        TotalSize = totalSize;
        ModifiedDate = modifiedDate;
        Revision = revision;
        AddedFiles = addedFiles;
        AddedFileChecksums = addedFileChecksums;
        RemovedFiles = removedFiles;
        Assets = assets;
    }

    public string ID { get; }
    public int ContainerVersion { get; }
    public string? Name { get; }
    public string? Description { get; }
    public string? Author { get; }
    public PatchFlags Flags { get; }
    public long TotalSize { get; }
    public DateTime ModifiedDate { get; }
    public int Revision { get; }
    public string[]? AddedFiles { get; }
    public string[]? AddedFileChecksums { get; }
    public string[]? RemovedFiles { get; }
    public string[]? Assets { get; } // Optional asset files, such as a thumbnail

    public bool HasAsset(string assetName) => Assets?.Contains(assetName) == true;
}