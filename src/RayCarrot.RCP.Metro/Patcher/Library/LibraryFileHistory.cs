using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.Library;

/// <summary>
/// Keeps track of the performed file modifications
/// </summary>
/// <param name="AddedFiles">The added files</param>
/// <param name="ReplacedFiles">The replaced files. The original files will have been saved in the file history.</param>
/// <param name="RemovedFiles">The removed files. The original files will have been saved in the file history.</param>
public record LibraryFileHistory(
    [JsonProperty("added_files", Required = Required.Always)] PatchFilePath[] AddedFiles,
    [JsonProperty("replaced_files", Required = Required.Always)] PatchFilePath[] ReplacedFiles,
    [JsonProperty("removed_files", Required = Required.Always)] PatchFilePath[] RemovedFiles);