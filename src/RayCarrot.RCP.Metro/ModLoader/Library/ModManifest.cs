using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

/// <summary>
/// The manifest of installed mods
/// </summary>
/// <param name="Mods">The installed mods</param>
public record ModManifest(
    [property: JsonProperty("mods", Required = Required.Always)] Dictionary<string, ModManifestEntry> Mods);