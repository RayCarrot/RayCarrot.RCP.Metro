using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.Library;

/// <summary>
/// The manifest of installed patches
/// </summary>
/// <param name="Patches">The installed patches</param>
public record PatchManifest(
    [JsonProperty("patches", Required = Required.Always)] Dictionary<string, PatchManifestEntry> Patches);