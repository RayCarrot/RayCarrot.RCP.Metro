using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

/// <summary>
/// The manifest entry for an installed mod
/// </summary>
/// <param name="Id">The mod id</param>
/// <param name="Size">The total installed size of the mod</param>
/// <param name="IsEnabled">Indicates if the mod is currently enabled</param>
/// <param name="Version">The currently selected mod version if it's enabled</param>
public record ModManifestEntry(
    [JsonProperty("id", Required = Required.Always)] string Id,
    [JsonProperty("size")] long Size,
    [JsonProperty("enabled")] bool IsEnabled,
    [JsonProperty("version")] string? Version);