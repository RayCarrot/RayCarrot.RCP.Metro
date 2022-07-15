using System.Collections.Generic;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher
{
    public record ExternalPatchesManifest(
        [JsonProperty(Required = Required.Always)]
        int ManifestVersion,

        Dictionary<Games, string>? Games)
    {
        public const int LatestVersion = 0;
    }
}
