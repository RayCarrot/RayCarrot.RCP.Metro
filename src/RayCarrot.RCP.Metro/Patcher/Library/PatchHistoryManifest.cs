using System;

namespace RayCarrot.RCP.Metro.Patcher;

// NOTE: Currently the checksum values are unused. The checksum gets calculated when creating a patch and then copied into
//       here, but not used for anything. The original intention was to use them for verifying that a file had not been
//       changed outside of the patch, but doing all of those calculations each time you opened the patcher would probably
//       be quite slow. So for now we keep them here, unused, until they might be needed in the future.

public record PatchHistoryManifest(
    DateTime ModifiedDate,

    // Files added to the game. No data is saved for these. Restore by deleting them.
    PatchFilePath[]? AddedFiles,
    byte[][]? AddedFileChecksums,

    // Files replaced in the game. The original file is saved. Restore by replacing it back.
    PatchFilePath[]? ReplacedFiles,
    byte[][]? ReplacedFileChecksums,

    // Files removed from the game. The original file is saved. Restore by adding it back.
    PatchFilePath[]? RemovedFiles);