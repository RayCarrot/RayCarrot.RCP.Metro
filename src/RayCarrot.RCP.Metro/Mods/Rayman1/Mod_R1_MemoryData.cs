using System.Collections.Generic;
using BinarySerializer;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_MemoryData : Mod_MemoryData
{
    public StatusBar? StatusBar { get; set; }
    public Poing? Poing { get; set; }
    public ObjData? Ray { get; set; }
    public R2_ObjData? R2_Ray { get; set; }
    public short HelicoTime { get; set; }

    public WorldInfo[]? WorldInfo { get; set; } // World map entries
    public byte RandomIndex { get; set; } // Index to the random array

    public RayMode RayMode { get; set; }
    public byte RayModeSpeed { get; set; }
    public RayEvts RayEvts { get; set; }
    public R2_RayEvts R2_RayEvts { get; set; }

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

    public static Dictionary<string, long> Offsets_PS1_R2 => new()
    {
        [nameof(StatusBar)] = 0x80145a90,
        [nameof(Poing)] = 0x8014a828,
        [nameof(R2_Ray)] = 0x80178df0,
        [nameof(HelicoTime)] = 0x80145a0a,

        //[nameof(WorldInfo)] = ,
        [nameof(RandomIndex)] = 0x80145860,

        [nameof(RayMode)] = 0x80145b20,
        [nameof(RayModeSpeed)] = 0x801459b2,
        [nameof(R2_RayEvts)] = 0x80145bd0,

        [nameof(MapTime)] = 0x80145b1c,
        [nameof(ActiveObjects)] = 0x80175740,
        [nameof(XMap)] = 0x80145b9c,
        [nameof(YMap)] = 0x80145b9e,

        //[nameof(AllWorld)] = ,
        [nameof(NumLevelChoice)] = 0x80145a26,
        [nameof(NumWorldChoice)] = 0x80145a3a,
        [nameof(NumLevel)] = 0x80145bb8,
        [nameof(NumWorld)] = 0x80145bda,
        //[nameof(NewLevel)] = ,
        //[nameof(NewWorld)] = ,
        [nameof(MenuEtape)] = 0x80145a2a,

        //[nameof(FinBoss)] = ,
        //[nameof(FinBossLevel)] = ,

        // TODO: Language
        // TODO: Hitboxes
        // TODO: Text
        // TODO: Play demo?
        // TODO: Cheat menu?
    };

    protected override void SerializeImpl(SerializerObject s)
    {
        StatusBar = SerializeObject<StatusBar>(s, StatusBar, name: nameof(StatusBar));
        Poing = SerializeObject<Poing>(s, Poing, name: nameof(Poing));
        Ray = SerializeObject<ObjData>(s, Ray, onPreSerialize: x => x.Pre_IsSerializingFromMemory = true, name: nameof(Ray));
        R2_Ray = SerializeObject<R2_ObjData>(s, R2_Ray, onPreSerialize: x => x.Pre_IsSerializingFromMemory = true, name: nameof(R2_Ray));
        HelicoTime = Serialize<short>(s, HelicoTime, name: nameof(HelicoTime));

        WorldInfo = SerializeObjectArray<WorldInfo>(s, WorldInfo, 24, name: nameof(WorldInfo));
        RandomIndex = Serialize<byte>(s, RandomIndex, name: nameof(RandomIndex));

        RayMode = Serialize<RayMode>(s, RayMode, name: nameof(RayMode));
        RayModeSpeed = Serialize<byte>(s, RayModeSpeed, name: nameof(RayModeSpeed));
        RayEvts = Serialize<RayEvts>(s, RayEvts, name: nameof(RayEvts));
        R2_RayEvts = Serialize<R2_RayEvts>(s, R2_RayEvts, name: nameof(R2_RayEvts));

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