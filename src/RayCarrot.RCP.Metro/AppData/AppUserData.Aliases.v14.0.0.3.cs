#nullable disable
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

public partial class AppUserData : BaseViewModel
{
#pragma warning disable IDE0051 // Remove unused private members
    [JsonProperty] private bool Patcher_LoadExternalPatches { set => ModLoader_LoadExternalMods = value; }
#pragma warning restore IDE0051 // Remove unused private members
}