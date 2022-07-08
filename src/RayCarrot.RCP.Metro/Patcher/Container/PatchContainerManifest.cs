using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record PatchContainerManifest(
    [property: JsonProperty(Required = Required.Always)]
    int ContainerVersion,

    [property: JsonProperty(Required = Required.Always)]
    PatchHistoryManifest History,

    [property: JsonProperty(Required = Required.Always)]
    PatchManifest[] Patches,

    string[]? EnabledPatches);