using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.FileInfo;

/// <summary>
/// Defines informational data on the files in a patch
/// </summary>
/// <param name="Versions">The game versions which this patch has data defined for</param>
/// <param name="Archives">The game archive files which this patch has data defined for</param>
/// <param name="RemovedFiles">The game files which this patch removes</param>
public record PatchFilesInfo(
    [property: JsonProperty("versions")] string[]? Versions,
    [property: JsonProperty("archives")] PatchArchiveInfo[]? Archives,
    [property: JsonProperty("removed_files")] Dictionary<string, string[]>? RemovedFiles);