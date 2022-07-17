using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record PatchLibraryManifest(
    [property: JsonProperty(Required = Required.Always)]
    int LibraryVersion,

    [property: JsonProperty(Required = Required.Always)]
    Games Game,

    [property: JsonProperty(Required = Required.Always)]
    PatchHistoryManifest History,

    PatchManifest[]? Patches,
    string[]? EnabledPatches)
{
    public const int LatestVersion = 0;
}