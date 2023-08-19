using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Legacy.Patcher;

#nullable disable

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

    public override void SerializeImpl(SerializerObject s)
    {
        Location = s.SerializeString(Location, encoding: Encoding.UTF8, name: nameof(Location));
        LocationID = s.SerializeString(LocationID, encoding: Encoding.UTF8, name: nameof(LocationID));
        FilePath = s.SerializeString(FilePath, encoding: Encoding.UTF8, name: nameof(FilePath));
    }
}