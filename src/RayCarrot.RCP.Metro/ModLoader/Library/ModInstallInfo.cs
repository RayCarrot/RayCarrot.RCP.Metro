using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

/// <summary>
/// Information about the mod installation. This is normally not changed after it's been installed.
/// </summary>
/// <param name="Source">The id of the source the mod was downloaded from. If null then the mod was added locally.</param>
/// <param name="Size">The total installed size of the mod</param>
/// <param name="Date">The date the mod was installed</param>
/// <param name="Data">Optional installation data. This is managed by the source. If the source is null then this is always null.</param>
public record ModInstallInfo(
    [property: JsonProperty("source")] string? Source, // Null if installed locally
    [property: JsonProperty("size")] long Size,
    [property: JsonProperty("date")] DateTime? Date,
    [property: JsonProperty("data")] JObject? Data); // Data type depends on the source