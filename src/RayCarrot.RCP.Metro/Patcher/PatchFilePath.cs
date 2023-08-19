using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

[JsonConverter(typeof(PatchFilePathJsonConverter))]
public readonly struct PatchFilePath
{
    public PatchFilePath(string filePath)
    {
        FilePath = filePath;
        Location = String.Empty;
        LocationID = String.Empty;
    }
    public PatchFilePath(string filePath, string location, string locationId)
    {
        FilePath = filePath;
        Location = location;
        LocationID = locationId;
    }

    public string FilePath { get; }
    public string Location { get; }
    public string LocationID { get; }

    public bool HasLocation => Location != String.Empty;
    public string FullFilePath => HasLocation ? System.IO.Path.Combine(Location, FilePath) : FilePath;

    public override string ToString() => HasLocation ? $"{Location}:{FilePath}" : FilePath;
}