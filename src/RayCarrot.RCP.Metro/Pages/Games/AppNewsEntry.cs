using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.Pages.Games;

public record AppNewsEntry(
    [property: JsonProperty("minAppVersion"), JsonConverter(typeof(VersionConverter))] Version? MinAppVersion,
    [property: JsonProperty("maxAppVersion"), JsonConverter(typeof(VersionConverter))] Version? MaxAppVersion,
    [property: JsonProperty("header")] string? Header,
    [property: JsonProperty("text")] string? Text);