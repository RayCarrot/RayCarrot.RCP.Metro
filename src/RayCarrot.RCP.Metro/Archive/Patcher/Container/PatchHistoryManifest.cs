﻿using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Archive;

// NOTE: Currently the checksum values are unused. The checksum gets calculated when creating a patch and then copied into
//       here, but not used for anything. The original intention was to use them for verifying that a file had not been
//       changed outside of the patch, but doing all of those calculations each time you opened the patcher would probably
//       be quite slow. So for now we keep them here, unused, until they might be needed in the future.

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