#nullable disable
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class RRR_SavSlotDesc : BinarySerializable
{
    public string Name { get; set; }
    public uint GameTime { get; set; } // Seconds since 1970
    public int Slot { get; set; }
    public int Map { get; set; }
    public int WP { get; set; }
    public int Time { get; set; } // Seconds since 1970, will break in 2038
    public byte Progress_Days { get; set; }
    public byte Progress_Percentage { get; set; }
    public byte Progress_Byte_02 { get; set; }
    public bool Progress_Byte_FinishedGame { get; set; }
    public int[] Score { get; set; }
    public int ID { get; set; }
    public int Tick { get; set; }

    // Only set in slot 4
    public int RenderMode { get; set; }
    public float UsrGroupVolume_3 { get; set; }
    public float UsrGroupVolume_0 { get; set; }
    public float UsrGroupVolume_2 { get; set; }
    public int DisplayMode { get; set; }
    public int Vibration { get; set; }
    public int IsAutoSaveDisabled { get; set; }

    public int Dummy { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Name = s.SerializeString(Name, 32, name: nameof(Name));
        GameTime = s.Serialize<uint>(GameTime, name: nameof(GameTime));
        Slot = s.Serialize<int>(Slot, name: nameof(Slot));
        Map = s.Serialize<int>(Map, name: nameof(Map));
        WP = s.Serialize<int>(WP, name: nameof(WP));
        Time = s.Serialize<int>(Time, name: nameof(Time));
        Progress_Days = s.Serialize<byte>(Progress_Days, name: nameof(Progress_Days));
        Progress_Percentage = s.Serialize<byte>(Progress_Percentage, name: nameof(Progress_Percentage));
        Progress_Byte_02 = s.Serialize<byte>(Progress_Byte_02, name: nameof(Progress_Byte_02));
        Progress_Byte_FinishedGame = s.Serialize<bool>(Progress_Byte_FinishedGame, name: nameof(Progress_Byte_FinishedGame));
        Score = s.SerializeArray<int>(Score, 40, name: nameof(Score));
        ID = s.Serialize<int>(ID, name: nameof(ID));
        Tick = s.Serialize<int>(Tick, name: nameof(Tick));
        RenderMode = s.Serialize<int>(RenderMode, name: nameof(RenderMode));
        UsrGroupVolume_3 = s.Serialize<float>(UsrGroupVolume_3, name: nameof(UsrGroupVolume_3));
        UsrGroupVolume_0 = s.Serialize<float>(UsrGroupVolume_0, name: nameof(UsrGroupVolume_0));
        UsrGroupVolume_2 = s.Serialize<float>(UsrGroupVolume_2, name: nameof(UsrGroupVolume_2));
        DisplayMode = s.Serialize<int>(DisplayMode, name: nameof(DisplayMode));
        Vibration = s.Serialize<int>(Vibration, name: nameof(Vibration));
        IsAutoSaveDisabled = s.Serialize<int>(IsAutoSaveDisabled, name: nameof(IsAutoSaveDisabled));
        Dummy = s.Serialize<int>(Dummy, name: nameof(Dummy));
    }
}