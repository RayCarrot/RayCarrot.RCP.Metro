using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Archive;

public record PatchHistoryManifest(
    [property: JsonProperty(Required = Required.Always)]
    string ID,

    long TotalSize, 
    DateTime ModifiedDate, 
    
    // Files added to the archive. No data is saved for these. Restore by deleting them.
    string[]? AddedFiles,
    string[]? AddedFileChecksums, 
    
    // Files replaced in the archive. The original file is saved. Restore by replacing it back.
    string[]? ReplacedFiles,
    string[]? ReplacedFileChecksums, 
    
    // Files removed from the archive. The original file is saved. Restore by adding it back.
    string[]? RemovedFiles);