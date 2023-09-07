using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

/// <summary>
/// The manifest entry for an installed mod
/// </summary>
/// <param name="Id">The mod id</param>
/// <param name="Id">The mod id</param>
/// <param name="InstallInfo">Information about the mod installation. This is normally not changed after it's been installed.</param>
/// <param name="Version">The currently selected mod version if it's enabled</param>
public record ModManifestEntry(
    [property: JsonProperty("id", Required = Required.Always)] string Id,
    [property: JsonProperty("install_info", Required = Required.Always)] ModInstallInfo InstallInfo,
    [property: JsonProperty("enabled")] bool IsEnabled);