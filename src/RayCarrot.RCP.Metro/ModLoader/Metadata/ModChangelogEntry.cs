using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.ModLoader.Metadata;

public record ModChangelogEntry(
    [JsonProperty("version")] ModVersion? Version,
    [JsonProperty("date")] DateTime? Date,
    [JsonProperty("description")] string? Description);