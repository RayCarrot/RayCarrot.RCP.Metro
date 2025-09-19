using BinarySerializer;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Save data for Rayman Hoodlums' Revenge
/// </summary>
public class RHR_SaveData : BinarySerializable
{
    public RHR_SaveFlags Flags { get; set; }
    public RHR_Language Language { get; set; }
    public byte SfxVolume { get; set; }
    public byte MusicVolume { get; set; }
    public RHR_Highscore[] Highscores { get; set; }
    public RHR_SaveSlot[] Slots { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoProcessed(new Checksum16Processor(), p =>
        {
            p?.Serialize<ushort>(s, name: "SaveDataChecksum");
            s.SerializeMagic<byte>(0x47);

            Flags = s.Serialize<RHR_SaveFlags>(Flags, name: nameof(Flags));
            Language = s.Serialize<RHR_Language>(Language, name: nameof(Language));
            SfxVolume = s.Serialize<byte>(SfxVolume, name: nameof(SfxVolume));
            MusicVolume = s.Serialize<byte>(MusicVolume, name: nameof(MusicVolume));
            s.SerializePadding(1, logIfNotNull: true);
            Highscores = s.SerializeObjectArray<RHR_Highscore>(Highscores, 6, name: nameof(Highscores));
        });

        Slots = s.SerializeObjectArray<RHR_SaveSlot>(Slots, 4, name: nameof(Slots));
    }
}