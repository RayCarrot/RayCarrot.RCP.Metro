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
    public bool HasBeatTimeAttackBestLapTime { get; set; } // +0.3%
    public bool HasBeatChampionshipBestLapTime { get; set; } // +0.4%
    public bool HasBeatLumsBestLapTime { get; set; } // +0.3%
    public bool HasBeatTargetBestLapTime { get; set; } // +0.3%

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
        HasBeatTimeAttackBestLapTime = s.Serialize<bool>(HasBeatTimeAttackBestLapTime, name: nameof(HasBeatTimeAttackBestLapTime));
        HasBeatChampionshipBestLapTime = s.Serialize<bool>(HasBeatChampionshipBestLapTime, name: nameof(HasBeatChampionshipBestLapTime));
        HasBeatLumsBestLapTime = s.Serialize<bool>(HasBeatLumsBestLapTime, name: nameof(HasBeatLumsBestLapTime));
        HasBeatTargetBestLapTime = s.Serialize<bool>(HasBeatTargetBestLapTime, name: nameof(HasBeatTargetBestLapTime));
    }
}