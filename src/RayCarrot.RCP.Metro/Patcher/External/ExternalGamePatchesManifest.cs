using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalGamePatchesManifest(
    [JsonProperty(Required = Required.Always)]
    Games Game,

    ExternalPatchManifest[]? Patches);