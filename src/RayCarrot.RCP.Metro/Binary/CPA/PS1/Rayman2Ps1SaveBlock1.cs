using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class Rayman2Ps1SaveBlock1 : CPAPs1SaveBlock
{
    public override int AllocatedSize => 0x200;
    public override int ExpectedSize => 0x148;

    public short Short_00 { get; set; }
    public short Short_02 { get; set; }
    public byte[] Bytes_04 { get; set; }
    public int Int_08 { get; set; }
    public int HallOfDoorsIndex { get; set; }
    public int Int_10 { get; set; }
    public byte[] UnknownMapFlags { get; set; } // A flag for each map
    public byte[] CinematicFlags { get; set; } // A flag for each cutscene, indicating if it's been seen
    public int Int_28 { get; set; }
    public int Int_2C { get; set; } // A flag per level
    public byte Byte_30 { get; set; }
    public byte Byte_31 { get; set; } // Flags
    public byte[] Bytes_32 { get; set; } // Padding?
    public int Int_34 { get; set; }
    public byte[] LumFlags { get; set; } // One bit for each collected lum
    public byte[] LumCounts { get; set; } // Per level
    public byte[] CageFlags { get; set; } // One bit for each collected cage
    public byte[] CageCounts { get; set; } // Per level
    public byte[] CagedDenyCounts { get; set; } // Per level
    public byte[] CagedLudivCounts { get; set; } // Per level
    public byte[] CagedMurfyCounts { get; set; } // Per level
    public byte SoundEffectsVolume { get; set; }
    public byte MusicVolume { get; set; }
    public bool IsStereoSound { get; set; }
    public byte Byte_123 { get; set; }
    public uint RaceTime1 { get; set; }
    public uint RaceTime2 { get; set; }
    public int Int_12C { get; set; }
    public int TotalPlayTime { get; set; }
    public int Int_134 { get; set; }
    public int Language { get; set; }
    public int ScreenOffsetX { get; set; }
    public int ScreenOffsetY { get; set; }
    public byte Byte_144 { get; set; }
    public byte Byte_145 { get; set; }
    public byte[] Bytes_146 { get; set; } // Padding?

    protected override void SerializeData(SerializerObject s)
    {
        Short_00 = s.Serialize<short>(Short_00, name: nameof(Short_00));
        Short_02 = s.Serialize<short>(Short_02, name: nameof(Short_02));
        Bytes_04 = s.SerializeArray<byte>(Bytes_04, 4, name: nameof(Bytes_04));
        Int_08 = s.Serialize<int>(Int_08, name: nameof(Int_08));
        HallOfDoorsIndex = s.Serialize<int>(HallOfDoorsIndex, name: nameof(HallOfDoorsIndex));
        Int_10 = s.Serialize<int>(Int_10, name: nameof(Int_10));
        UnknownMapFlags = s.SerializeArray<byte>(UnknownMapFlags, 10, name: nameof(UnknownMapFlags));
        CinematicFlags = s.SerializeArray<byte>(CinematicFlags, 10, name: nameof(CinematicFlags));
        Int_28 = s.Serialize<int>(Int_28, name: nameof(Int_28));
        Int_2C = s.Serialize<int>(Int_2C, name: nameof(Int_2C));
        Byte_30 = s.Serialize<byte>(Byte_30, name: nameof(Byte_30));
        Byte_31 = s.Serialize<byte>(Byte_31, name: nameof(Byte_31));
        Bytes_32 = s.SerializeArray<byte>(Bytes_32, 2, name: nameof(Bytes_32));
        Int_34 = s.Serialize<int>(Int_34, name: nameof(Int_34));
        LumFlags = s.SerializeArray<byte>(LumFlags, 100, name: nameof(LumFlags));
        LumCounts = s.SerializeArray<byte>(LumCounts, 24, name: nameof(LumCounts));
        CageFlags = s.SerializeArray<byte>(CageFlags, 12, name: nameof(CageFlags));
        CageCounts = s.SerializeArray<byte>(CageCounts, 24, name: nameof(CageCounts));
        CagedDenyCounts = s.SerializeArray<byte>(CagedDenyCounts, 24, name: nameof(CagedDenyCounts));
        CagedLudivCounts = s.SerializeArray<byte>(CagedLudivCounts, 24, name: nameof(CagedLudivCounts));
        CagedMurfyCounts = s.SerializeArray<byte>(CagedMurfyCounts, 24, name: nameof(CagedMurfyCounts));
        SoundEffectsVolume = s.Serialize<byte>(SoundEffectsVolume, name: nameof(SoundEffectsVolume));
        MusicVolume = s.Serialize<byte>(MusicVolume, name: nameof(MusicVolume));
        IsStereoSound = s.Serialize<bool>(IsStereoSound, name: nameof(IsStereoSound));
        Byte_123 = s.Serialize<byte>(Byte_123, name: nameof(Byte_123));
        RaceTime1 = s.Serialize<uint>(RaceTime1, name: nameof(RaceTime1));
        RaceTime2 = s.Serialize<uint>(RaceTime2, name: nameof(RaceTime2));
        Int_12C = s.Serialize<int>(Int_12C, name: nameof(Int_12C));
        TotalPlayTime = s.Serialize<int>(TotalPlayTime, name: nameof(TotalPlayTime));
        Int_134 = s.Serialize<int>(Int_134, name: nameof(Int_134));
        Language = s.Serialize<int>(Language, name: nameof(Language));
        ScreenOffsetX = s.Serialize<int>(ScreenOffsetX, name: nameof(ScreenOffsetX));
        ScreenOffsetY = s.Serialize<int>(ScreenOffsetY, name: nameof(ScreenOffsetY));
        Byte_144 = s.Serialize<byte>(Byte_144, name: nameof(Byte_144));
        Byte_145 = s.Serialize<byte>(Byte_145, name: nameof(Byte_145));
        Bytes_146 = s.SerializeArray<byte>(Bytes_146, 2, name: nameof(Bytes_146));
    }
}