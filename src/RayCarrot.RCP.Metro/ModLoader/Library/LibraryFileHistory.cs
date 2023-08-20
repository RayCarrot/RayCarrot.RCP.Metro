using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

/// <summary>
/// Keeps track of the performed file modifications
/// </summary>
/// <param name="AddedFiles">The added files</param>
/// <param name="ReplacedFiles">The replaced files. The original files will have been saved in the file history.</param>
/// <param name="RemovedFiles">The removed files. The original files will have been saved in the file history.</param>
public record LibraryFileHistory(
    [property: JsonProperty("added_files", Required = Required.Always)] ModFilePath[] AddedFiles,
    [property: JsonProperty("replaced_files", Required = Required.Always)] ModFilePath[] ReplacedFiles,
    [property: JsonProperty("removed_files", Required = Required.Always)] ModFilePath[] RemovedFiles);