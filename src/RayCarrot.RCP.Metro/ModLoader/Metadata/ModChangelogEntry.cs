using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Metadata;

public record ModChangelogEntry(
    [property: JsonProperty("version")] ModVersion? Version,
    [property: JsonProperty("date")] DateTime? Date,
    [property: JsonProperty("description")] string? Description);