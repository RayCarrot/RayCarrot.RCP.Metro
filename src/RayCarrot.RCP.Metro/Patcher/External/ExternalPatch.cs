using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalPatch(
    [property: JsonProperty("rcp_minAppVersion")] [property: JsonConverter(typeof(VersionConverter))] Version? MinAppVersion,
    [property: JsonProperty("rcp_maxAppVersion")][property: JsonConverter(typeof(VersionConverter))] Version? MaxAppVersion,
    //[property: JsonProperty("rcp_appChannels")] string[]? AppChannels,

    [property: JsonProperty("metaData")] ExternalPatchMetaData? MetaData);