using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.FileInfo;

/// <summary>
/// Defines informational data on the files in a mod
/// </summary>
/// <param name="Versions">The game versions which this mod has data defined for</param>
/// <param name="Archives">The game archive files which this mod has data defined for</param>
/// <param name="RemovedFiles">The game files which this mod removes</param>
public record ModFilesInfo(
    [property: JsonProperty("versions")] string[]? Versions,
    [property: JsonProperty("archives")] ModArchiveInfo[]? Archives,
    [property: JsonProperty("removed_files")] Dictionary<string, string[]>? RemovedFiles);