using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO-14: Don't indent on web to save space and transfer time
public record ExternalPatchesManifest(
    [JsonProperty(Required = Required.Always)]
    int ManifestVersion,

    Dictionary<LegacyGame, string>? Games)
{
    public const int LatestVersion = 0;
}