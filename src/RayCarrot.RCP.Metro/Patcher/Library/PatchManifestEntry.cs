using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.Library;

/// <summary>
/// The manifest entry for an installed patch
/// </summary>
/// <param name="Id">The patch id</param>
/// <param name="Size">The total installed size of the patch</param>
/// <param name="IsEnabled">Indicates if the patch is currently enabled</param>
/// <param name="Version">The currently selected patch version if it's enabled</param>
public record PatchManifestEntry(
    [JsonProperty("id", Required = Required.Always)] string Id,
    [JsonProperty("size")] long Size,
    [JsonProperty("enabled")] bool IsEnabled,
    [JsonProperty("version")] string? Version);