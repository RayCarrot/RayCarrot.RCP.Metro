using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher.Metadata;

public record PatchChangelogEntry(
    [JsonProperty("version")] PatchVersion? Version,
    [JsonProperty("date")] DateTime? Date,
    [JsonProperty("description")] string? Description);