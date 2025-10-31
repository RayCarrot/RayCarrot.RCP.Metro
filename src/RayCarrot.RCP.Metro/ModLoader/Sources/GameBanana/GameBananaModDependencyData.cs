using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public record GameBananaModDependencyData(
    [property: JsonProperty("mod_id", Required = Required.Always)] long ModId,
    [property: JsonProperty("file_id")] long? FileId);