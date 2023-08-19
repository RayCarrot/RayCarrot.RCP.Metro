using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.FileInfo;

/// <summary>
/// Defines an archive file in a patch
/// </summary>
/// <param name="Id">The archive id. This is used to match it with the correct manager to handle its format.</param>
/// <param name="FilePath">The archive file path</param>
public record PatchArchiveInfo(
    [property: JsonProperty("id", Required = Required.Always)] string Id,
    [property: JsonProperty("path", Required = Required.Always)] string FilePath);