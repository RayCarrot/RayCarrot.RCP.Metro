using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO-14: Display this changelog in the ui. In patcher dialog? App home page?
public record ExternalPatchChangeLogEntry(
    [property: JsonProperty("version")] PatchVersion? Version,
    [property: JsonProperty("date")] DateTime? Date,
    [property: JsonProperty("changes")] string? Changes);