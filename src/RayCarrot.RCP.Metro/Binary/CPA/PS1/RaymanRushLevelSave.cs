#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RaymanRushLevelSave : BinarySerializable
{
    public int TimeAttackBestLapTime { get; set; }
    public int ChampionshipBestLapTime { get; set; }
    public int LumsBestLapTime { get; set; }
    public int TargetBestLapTime { get; set; }
    public int ChampionshipBestTotalTime { get; set; }
    public int LumsBestTotalTime { get; set; }
    public int TargetBestTotalTime { get; set; }
    public int[] Ints_1C { get; set; }
    public byte LumsCount { get; set; }
    public byte CompletedChampionship { get; set; } // Bool?
    public byte CompletedModes { get; set; } // 2 = Time Attack, 3 = Time Attack + Lums, 4 = Time Attack + Lums + Target
    public byte Byte_2F { get; set; }
    public byte[] Bytes_30 { get; set; }
    public byte Byte_38 { get; set; }
    public byte Byte_39 { get; set; }
    public byte Byte_3A { get; set; }
    public byte Byte_3B { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        TimeAttackBestLapTime = s.Serialize<int>(TimeAttackBestLapTime, name: nameof(TimeAttackBestLapTime));
        ChampionshipBestLapTime = s.Serialize<int>(ChampionshipBestLapTime, name: nameof(ChampionshipBestLapTime));
        LumsBestLapTime = s.Serialize<int>(LumsBestLapTime, name: nameof(LumsBestLapTime));
        TargetBestLapTime = s.Serialize<int>(TargetBestLapTime, name: nameof(TargetBestLapTime));
        ChampionshipBestTotalTime = s.Serialize<int>(ChampionshipBestTotalTime, name: nameof(ChampionshipBestTotalTime));
        LumsBestTotalTime = s.Serialize<int>(LumsBestTotalTime, name: nameof(LumsBestTotalTime));
        TargetBestTotalTime = s.Serialize<int>(TargetBestTotalTime, name: nameof(TargetBestTotalTime));
        Ints_1C = s.SerializeArray<int>(Ints_1C, 4, name: nameof(Ints_1C));
        LumsCount = s.Serialize<byte>(LumsCount, name: nameof(LumsCount));
        CompletedChampionship = s.Serialize<byte>(CompletedChampionship, name: nameof(CompletedChampionship));
        CompletedModes = s.Serialize<byte>(CompletedModes, name: nameof(CompletedModes));
        Byte_2F = s.Serialize<byte>(Byte_2F, name: nameof(Byte_2F));
        Bytes_30 = s.SerializeArray<byte>(Bytes_30, 8, name: nameof(Bytes_30));
        Byte_38 = s.Serialize<byte>(Byte_38, name: nameof(Byte_38));
        Byte_39 = s.Serialize<byte>(Byte_39, name: nameof(Byte_39));
        Byte_3A = s.Serialize<byte>(Byte_3A, name: nameof(Byte_3A));
        Byte_3B = s.Serialize<byte>(Byte_3B, name: nameof(Byte_3B));
    }
}