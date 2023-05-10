using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalPatchChangeLogEntry(
    [property: JsonProperty("version")] PatchVersion? Version,
    [property: JsonProperty("date")] DateTime? Date,
    [property: JsonProperty("description")] string? Description);