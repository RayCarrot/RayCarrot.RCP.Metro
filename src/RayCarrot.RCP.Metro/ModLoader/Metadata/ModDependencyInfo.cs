using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro.ModLoader.Metadata;

public record ModDependencyInfo(
    [property: JsonProperty("ids", Required = Required.Always)] string[] Ids,
    [property: JsonProperty("name", Required = Required.Always)] string Name,
    [property: JsonProperty("source_id")] string? SourceId,
    [property: JsonProperty("source_data")] JObject? SourceData);