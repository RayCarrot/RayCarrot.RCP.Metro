#nullable disable
using RayCarrot.Binary;

namespace RayCarrot.RCP.Metro;

public class JADE_BIG_FatFile : IBinarySerializable
{
    public static uint HeaderLength => 0x18;

    public JADE_BIG_BigFile Pre_Big { get; set; } // Set in OnPreSerialize

    public uint MaxFile { get; set; } // File count
    public uint MaxDir { get; set; } // Directory count
    public uint PosFat { get; set; } // offset of file list
    public int NextPosFat { get; set; }
    public uint FirstIndex { get; set; }
    public uint LastIndex { get; set; }

    public FileReference[] Files { get; set; }

    public void Serialize(IBinarySerializer s)
    {
        MaxFile = s.Serialize<uint>(MaxFile, name: nameof(MaxFile));
        MaxDir = s.Serialize<uint>(MaxDir, name: nameof(MaxDir));
        PosFat = s.Serialize<uint>(PosFat, name: nameof(PosFat));
        NextPosFat = s.Serialize<int>(NextPosFat, name: nameof(NextPosFat));
        FirstIndex = s.Serialize<uint>(FirstIndex, name: nameof(FirstIndex));
        LastIndex = s.Serialize<uint>(LastIndex, name: nameof(LastIndex));

        s.Stream.Position = PosFat;
        Files = s.SerializeObjectArray<FileReference>(Files, (int)MaxFile, name: nameof(Files));

        if (NextPosFat != -1)
            s.Stream.Position = NextPosFat - HeaderLength;
    }

    public class FileReference : IBinarySerializable
    {
        public static uint StructSize => 0x8;

        public uint FileOffset { get; set; }
        public uint Key { get; set; }

        public void Serialize(IBinarySerializer s)
        {
            FileOffset = s.Serialize<uint>(FileOffset, name: nameof(FileOffset));
            Key = s.Serialize<uint>(Key, name: nameof(Key));
        }
    }
}