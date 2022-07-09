using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

[JsonConverter(typeof(PatchFilePathJsonConverter))]
public readonly struct PatchFilePath
{
    public PatchFilePath(string location, string filePath)
    {
        Location = location;
        FilePath = filePath;
    }

    public string Location { get; }
    public string FilePath { get; }

    [JsonIgnore] 
    public bool HasLocation => !Location.IsNullOrEmpty();

    [JsonIgnore] 
    public string FullFilePath => HasLocation ? System.IO.Path.Combine(Location, FilePath) : FilePath;

    [JsonIgnore] 
    public string NormalizedFullFilePath => FullFilePath.ToLowerInvariant().Replace('\\', '/');

    public override string ToString() => HasLocation ? $"{Location}:{FilePath}" : FilePath;
}