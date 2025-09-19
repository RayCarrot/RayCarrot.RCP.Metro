using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

public partial class AppUserData : BaseViewModel
{
#pragma warning disable IDE0051 // Remove unused private members
    [JsonProperty] private Dictionary<string, FileSystemPath> Archive_AssociatedPrograms { set => FileEditors_AssociatedEditors = value; }
#pragma warning restore IDE0051 // Remove unused private members
}