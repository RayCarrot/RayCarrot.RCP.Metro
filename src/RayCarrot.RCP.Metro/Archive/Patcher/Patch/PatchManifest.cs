using System;
using System.Linq;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifest(
    string ID, 
    int ContainerVersion, 
    string? Name, 
    string? Description, 
    string? Author, 
    long TotalSize, 
    DateTime ModifiedDate, 
    int Revision, 
    string[]? AddedFiles,
    string[]? AddedFileChecksums, 
    string[]? RemovedFiles, 
    string[]? Assets)  // Optional asset files, such as a thumbnail
{
    public bool HasAsset(string assetName) => Assets?.Contains(assetName) == true;
}