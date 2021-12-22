#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Unity_PlayerPrefs : BinarySerializable
{
    public int FileSize { get; set; }
    public Unity_PlayerPrefsEntry[] Entries { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoChecksum(new ChecksumCRC32Calculator(), () =>
        {
            FileSize = s.Serialize<int>(FileSize, name: nameof(FileSize));

            Entries = s.SerializeObjectArrayUntil<Unity_PlayerPrefsEntry>(Entries, _ => s.CurrentFileOffset >= s.CurrentLength, name: nameof(Entries));
        }, ChecksumPlacement.Before, name: "Checksum");
    }
}