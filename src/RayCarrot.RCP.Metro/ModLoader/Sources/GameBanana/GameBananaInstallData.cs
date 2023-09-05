using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

/// <summary>
/// Mod installation data for GameBanana downloads
/// </summary>
/// <param name="ModId">The GameBanana mod id</param>
/// <param name="FileId">The GameBanana file id</param>
public record GameBananaInstallData(
    [property: JsonProperty("mod_id")] long ModId,
    [property: JsonProperty("file_id")] long FileId);