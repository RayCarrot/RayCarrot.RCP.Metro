﻿using System;
using System.Linq;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchManifest(
    string ID,
    // The version of the patch file format. This is different from revision which is the revision of the user-created patch.
    int PatchVersion,
    string? Name, 
    string? Description, 
    string? Author, 
    long TotalSize, 
    DateTime ModifiedDate, 
    int Revision, 
    string[]? AddedFiles,
    string[]? AddedFileChecksums, 
    string[]? RemovedFiles,
    // Optional asset files, such as a thumbnail
    string[]? Assets) 
{
    public bool HasAsset(string assetName) => Assets?.Contains(assetName) == true;
}