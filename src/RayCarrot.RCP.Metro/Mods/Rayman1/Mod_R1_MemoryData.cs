using System.Collections.Generic;
using BinarySerializer;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_MemoryData : Mod_MemoryData
{
    public StatusBar? StatusBar { get; set; }
    public Poing? Poing { get; set; }
    public ObjData? Ray { get; set; }
    public short HelicoTime { get; set; }

    public WorldInfo[]? WorldInfo { get; set; } // World map entries
    public byte RandomIndex { get; set; } // Index to the random array

    public RayMode RayMode { get; set; }
    public byte RayModeSpeed { get; set; }
    public RayEvts RayEvts { get; set; }

    public int MapTime { get; set; }
    public short[]? ActiveObjects { get; set; }
    public short XMap { get; set; }
    public short YMap { get; set; }

    public bool AllWorld { get; set; }
    public short NumLevelChoice { get; set; }
    public short NumWorldChoice { get; set; }
    public short NumLevel { get; set; }
    public short NumWorld { get; set; }
    public short NewLevel { get; set; }
    public short NewWorld { get; set; }
    public byte MenuEtape { get; set; }

    public bool FinBoss { get; set; }
    public FinBossLevel FinBossLevel { get; set; }

    public static Dictionary<string, long> Offsets_PC_1_21 => new()
    {
        [nameof(StatusBar)] = 0x16FF52,
        [nameof(Poing)] = 0x16F770,
        [nameof(Ray)] = 0x16F650,
        [nameof(HelicoTime)] = 0x170898,

        [nameof(WorldInfo)] = 0x135D5C,
        [nameof(RandomIndex)] = 0x18648C,

        [nameof(RayMode)] = 0x170868,
        [nameof(RayModeSpeed)] = 0x170A73,
        [nameof(RayEvts)] = 0x17081A,

        [nameof(MapTime)] = 0x16E8C0,
        [nameof(ActiveObjects)] = 0x16FF5C,
        [nameof(XMap)] = 0x17089E,
        [nameof(YMap)] = 0x1708A6,

        [nameof(AllWorld)] = 0x170A61,
        [nameof(NumLevelChoice)] = 0x17082E,
        [nameof(NumWorldChoice)] = 0x17083A,
        [nameof(NumLevel)] = 0x17087C,
        [nameof(NumWorld)] = 0x17088C,
        [nameof(NewLevel)] = 0x170880,
        [nameof(NewWorld)] = 0x170892,
        [nameof(MenuEtape)] = 0x170A37,

        [nameof(FinBoss)] = 0x170A76,
        [nameof(FinBossLevel)] = 0x17081E,
    };

    public static Dictionary<string, long> Offsets_PS1_US => new()
    {
        [nameof(StatusBar)] = 0x801e4d50,
        [nameof(Poing)] = 0x801d8b28,
        [nameof(Ray)] = 0x801f61a0,
        [nameof(HelicoTime)] = 0x801e5750,

        [nameof(WorldInfo)] = 0x801c335c,
        [nameof(RandomIndex)] = 0x801f5170,

        [nameof(RayMode)] = 0x801E5420,
        [nameof(RayModeSpeed)] = 0x801e4dd8,
        [nameof(RayEvts)] = 0x801f43d0,

        [nameof(MapTime)] = 0x801f6220,
        [nameof(ActiveObjects)] = 0x801e5428,
        [nameof(XMap)] = 0x801f84b8,
        [nameof(YMap)] = 0x801f84c0,

        //[nameof(AllWorld)] = ,
        [nameof(NumLevelChoice)] = 0x801e5a20,
        [nameof(NumWorldChoice)] = 0x801e63e8,
        [nameof(NumLevel)] = 0x801f9a68,
        [nameof(NumWorld)] = 0x801fa688,
        [nameof(NewLevel)] = 0x801f99f0,
        [nameof(NewWorld)] = 0x801fa5a8,
        [nameof(MenuEtape)] = 0x801f81a0,

        [nameof(FinBoss)] = 0x801f7a48,
        [nameof(FinBossLevel)] = 0x801f4ee8,
    };

    protected override void SerializeImpl(SerializerObject s)
    {
        StatusBar = SerializeObject<StatusBar>(s, StatusBar, name: nameof(StatusBar));
        Poing = SerializeObject<Poing>(s, Poing, name: nameof(Poing));
        Ray = SerializeObject<ObjData>(s, Ray, onPreSerialize: x => x.Pre_IsSerializingFromMemory = true, name: nameof(Ray));
        HelicoTime = Serialize<short>(s, HelicoTime, name: nameof(HelicoTime));

        WorldInfo = SerializeObjectArray<WorldInfo>(s, WorldInfo, 24, name: nameof(WorldInfo));
        RandomIndex = Serialize<byte>(s, RandomIndex, name: nameof(RandomIndex));

        RayMode = Serialize<RayMode>(s, RayMode, name: nameof(RayMode));
        RayModeSpeed = Serialize<byte>(s, RayModeSpeed, name: nameof(RayModeSpeed));
        RayEvts = Serialize<RayEvts>(s, RayEvts, name: nameof(RayEvts));

        MapTime = Serialize<int>(s, MapTime, name: nameof(MapTime));
        ActiveObjects = SerializeArray<short>(s, ActiveObjects, 112, name: nameof(ActiveObjects));
        XMap = Serialize<short>(s, XMap, name: nameof(XMap));
        YMap = Serialize<short>(s, YMap, name: nameof(YMap));

        AllWorld = Serialize<bool>(s, AllWorld, name: nameof(AllWorld));
        NumLevelChoice = Serialize<short>(s, NumLevelChoice, name: nameof(NumLevelChoice));
        NumWorldChoice = Serialize<short>(s, NumWorldChoice, name: nameof(NumWorldChoice));
        NumLevel = Serialize<short>(s, NumLevel, name: nameof(NumLevel));
        NumWorld = Serialize<short>(s, NumWorld, name: nameof(NumWorld));
        NewLevel = Serialize<short>(s, NewLevel, name: nameof(NewLevel));
        NewWorld = Serialize<short>(s, NewWorld, name: nameof(NewWorld));
        MenuEtape = Serialize<byte>(s, MenuEtape, name: nameof(MenuEtape));

        FinBoss = Serialize<bool>(s, FinBoss, name: nameof(FinBoss));
        FinBossLevel = Serialize<FinBossLevel>(s, FinBossLevel, name: nameof(FinBossLevel));
    }
}