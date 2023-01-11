using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record ExternalPatchesManifest(
    [property: JsonProperty("patches")] ExternalPatch?[]? Patches);