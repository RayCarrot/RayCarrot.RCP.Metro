#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Jade_BIG_FatFile : BinarySerializable
{
    public static uint HeaderLength => 0x18;

    public Jade_BIG_BigFile Pre_Big { get; set; } // Set in OnPreSerialize

    public uint MaxFile { get; set; } // File count
    public uint MaxDir { get; set; } // Directory count
    public uint PosFat { get; set; } // Offset of file list
    public int NextPosFat { get; set; }
    public uint FirstIndex { get; set; }
    public uint LastIndex { get; set; }

    public FileReference[] Files { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        MaxFile = s.Serialize<uint>(MaxFile, name: nameof(MaxFile));
        MaxDir = s.Serialize<uint>(MaxDir, name: nameof(MaxDir));
        PosFat = s.Serialize<uint>(PosFat, name: nameof(PosFat));
        NextPosFat = s.Serialize<int>(NextPosFat, name: nameof(NextPosFat));
        FirstIndex = s.Serialize<uint>(FirstIndex, name: nameof(FirstIndex));
        LastIndex = s.Serialize<uint>(LastIndex, name: nameof(LastIndex));

        s.Goto(new Pointer(PosFat, Offset.File));
        Files = s.SerializeObjectArray<FileReference>(Files, (int)MaxFile, name: nameof(Files));

        if (NextPosFat != -1)
            s.Goto(new Pointer(NextPosFat - HeaderLength, Offset.File));
    }

    public class FileReference : BinarySerializable
    {
        public uint FileOffset { get; set; }
        public uint Key { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            FileOffset = s.Serialize<uint>(FileOffset, name: nameof(FileOffset));
            Key = s.Serialize<uint>(Key, name: nameof(Key));
        }
    }
}