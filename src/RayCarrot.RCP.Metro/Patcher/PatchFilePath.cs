using System;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

[JsonConverter(typeof(PatchFilePathJsonConverter))]
public readonly struct PatchFilePath
{
    public PatchFilePath(string? location, string? locationId, string filePath)
    {
        Location = location ?? String.Empty;
        LocationID = locationId ?? String.Empty;
        FilePath = filePath;
    }

    public string Location { get; }
    public string LocationID { get; }
    public string FilePath { get; }

    public bool HasLocation => Location != String.Empty;
    public string FullFilePath => HasLocation ? System.IO.Path.Combine(Location, FilePath) : FilePath;
    public string NormalizedFullFilePath => FullFilePath.ToLowerInvariant().Replace('\\', '/');

    public override string ToString() => HasLocation ? $"{Location}:{FilePath}" : FilePath;
}