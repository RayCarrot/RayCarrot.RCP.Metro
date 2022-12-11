using System.Text;
using BinarySerializer;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

#nullable disable

[JsonConverter(typeof(PatchFilePathJsonConverter))]
public class PatchFilePath : BinarySerializable
{
    public PatchFilePath()
    {
        
    }

    public PatchFilePath(string location, string locationId, string filePath)
    {
        Location = location ?? String.Empty;
        LocationID = locationId ?? String.Empty;
        FilePath = filePath;
    }

    public string Location { get; private set; }
    public string LocationID { get; private set; }
    public string FilePath { get; private set; }

    public bool HasLocation => Location != String.Empty;
    public string FullFilePath => HasLocation ? System.IO.Path.Combine(Location, FilePath) : FilePath;
    public string NormalizedFullFilePath => FullFilePath.ToLowerInvariant().Replace('\\', '/');

    public override void SerializeImpl(SerializerObject s)
    {
        Location = s.SerializeString(Location, encoding: Encoding.UTF8, name: nameof(Location));
        LocationID = s.SerializeString(LocationID, encoding: Encoding.UTF8, name: nameof(LocationID));
        FilePath = s.SerializeString(FilePath, encoding: Encoding.UTF8, name: nameof(FilePath));
    }

    public override string ToString() => HasLocation ? $"{Location}:{FilePath}" : FilePath;
}