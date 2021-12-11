#nullable disable
using System.Collections.Generic;
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class Unity_PlayerPrefs : IBinarySerializable
{
    public int Checksum { get; set; }
    public int FileSize { get; set; }
    public Unity_PlayerPrefsEntry[] Entries { get; set; }

    public void Serialize(IBinarySerializer s)
    {
        Checksum = s.Serialize<int>(Checksum, name: nameof(Checksum));
        FileSize = s.Serialize<int>(FileSize, name: nameof(FileSize));

        if (s.IsReading)
        {
            List<Unity_PlayerPrefsEntry> entries = new();

            int index = 0;

            while (s.Stream.Position < s.Stream.Length)
            {
                entries.Add(s.SerializeObject<Unity_PlayerPrefsEntry>(default, name: $"{nameof(entries)}[{index}]"));
                index++;
            }

            Entries = entries.ToArray();
        }
        else
        {
            Entries = s.SerializeObjectArray<Unity_PlayerPrefsEntry>(Entries, Entries.Length, name: nameof(Entries));
        }
    }
}