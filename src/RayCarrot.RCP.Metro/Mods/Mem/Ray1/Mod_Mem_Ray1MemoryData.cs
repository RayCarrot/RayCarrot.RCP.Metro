using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Mod_Mem_Ray1MemoryData : Mod_Mem_MemoryData
{
    public StatusBar? StatusBar { get; set; }
    public Poing? Poing { get; set; }
    public ObjData? Ray { get; set; }
    public R2_ObjData? R2_Ray { get; set; }
    public short HelicoTime { get; set; }

    public WorldInfo[]? WorldInfo { get; set; } // World map entries
    public byte RandomIndex { get; set; } // Index to the random array

    public RayMode RayMode { get; set; }
    public byte RayModeSpeed { get; set; } // IDEA: Add? This is for place ray.
    public RayEvts? RayEvts { get; set; }

    public int MapTime { get; set; }
    public ActiveTable? ActiveObjects { get; set; }
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

    public int R2_Language { get; set; }
    public bool R2_ShowEngineInfo { get; set; }
    public bool R2_DebugMode { get; set; }
    public uint R2_UnusedMapLoopFunctionCall { get; set; }
    public uint R2_MapInitFunctionCall { get; set; } // Not unused, but it only resets the random index so not very important

    // The menus use the string at 0x080F2154 which is sadly empty. We could set it to something?
    public bool GBA_EnableMultiplayerMenus { get; set; } // Loads multiplayer menus on start
    public ushort GBA_MultiplayerTimeout { get; set; } // Set to 0 to disable the code
    public ushort GBA_MultiplayerLevelLoad { get; set; } // Set to 0 to allow menu to load level
    public bool GBA_MultiplayerPlayerSelection { get; set; }
    public byte GBA_MultiplayerLevelSelection { get; set; } // 1-6

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
        [nameof(RayEvts)] = 0x80145bd0,

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

        [nameof(R2_Language)] = 0x80145974,
        [nameof(R2_ShowEngineInfo)] = 0x801459b3,
        [nameof(R2_DebugMode)] = 0x80145a58,

        [nameof(R2_UnusedMapLoopFunctionCall)] = 0x801164c8,
        [nameof(R2_MapInitFunctionCall)] = 0x800e7c18,
    };

    public static Dictionary<string, long> Offsets_GBA_EU => new()
    {
        [nameof(StatusBar)] = 0x0202eea0,
        [nameof(Poing)] = 0x0202ee80,
        [nameof(Ray)] = 0x0202fc90,
        [nameof(HelicoTime)] = 0x0202fa98,

        [nameof(WorldInfo)] = 0x0202a2d0,
        [nameof(RandomIndex)] = 0x02030894,

        [nameof(RayMode)] = 0x020307f0,
        [nameof(RayModeSpeed)] = 0x0202fb60,
        [nameof(RayEvts)] = 0x0202bee4,

        [nameof(MapTime)] = 0x0202e51c,
        [nameof(ActiveObjects)] = 0x0202fd90,
        [nameof(XMap)] = 0x0202a508,
        [nameof(YMap)] = 0x0202a58c,

        //[nameof(AllWorld)] = ,
        [nameof(NumLevelChoice)] = 0x0202fb7c,
        [nameof(NumWorldChoice)] = 0x0202fa94,
        [nameof(NumLevel)] = 0x0202e5f0,
        [nameof(NumWorld)] = 0x02030394,
        [nameof(NewLevel)] = 0x0202bf10,
        [nameof(NewWorld)] = 0x0203038c,
        //[nameof(MenuEtape)] = ,

        [nameof(FinBoss)] = 0x02030810,
        [nameof(FinBossLevel)] = 0x0202fbf0,

        [nameof(GBA_EnableMultiplayerMenus)] = 0x02030f14,
        [nameof(GBA_MultiplayerTimeout)] = 0x08000ff0,
        [nameof(GBA_MultiplayerLevelLoad)] = 0x08045234,
        [nameof(GBA_MultiplayerPlayerSelection)] = 0x02021e2c,
        [nameof(GBA_MultiplayerLevelSelection)] = 0x02001178,
    };

    protected override void SerializeImpl()
    {
        StatusBar = SerializeObject<StatusBar>(StatusBar, name: nameof(StatusBar));
        Poing = SerializeObject<Poing>(Poing, name: nameof(Poing));
        Ray = SerializeObject<ObjData>(Ray, name: nameof(Ray));
        R2_Ray = SerializeObject<R2_ObjData>(R2_Ray, name: nameof(R2_Ray));
        HelicoTime = Serialize<short>(HelicoTime, name: nameof(HelicoTime));

        WorldInfo = SerializeObjectArray<WorldInfo>(WorldInfo, 24, name: nameof(WorldInfo));
        RandomIndex = Serialize<byte>(RandomIndex, name: nameof(RandomIndex));

        RayMode = Serialize<RayMode>(RayMode, name: nameof(RayMode));
        RayModeSpeed = Serialize<byte>(RayModeSpeed, name: nameof(RayModeSpeed));
        RayEvts = SerializeObject<RayEvts>(RayEvts, name: nameof(RayEvts));

        MapTime = Serialize<int>(MapTime, name: nameof(MapTime));
        ActiveObjects = SerializeObject<ActiveTable>(ActiveObjects, name: nameof(ActiveObjects));
        XMap = Serialize<short>(XMap, name: nameof(XMap));
        YMap = Serialize<short>(YMap, name: nameof(YMap));

        AllWorld = Serialize<bool>(AllWorld, name: nameof(AllWorld));
        NumLevelChoice = Serialize<short>(NumLevelChoice, name: nameof(NumLevelChoice));
        NumWorldChoice = Serialize<short>(NumWorldChoice, name: nameof(NumWorldChoice));
        NumLevel = Serialize<short>(NumLevel, name: nameof(NumLevel));
        NumWorld = Serialize<short>(NumWorld, name: nameof(NumWorld));
        NewLevel = Serialize<short>(NewLevel, name: nameof(NewLevel));
        NewWorld = Serialize<short>(NewWorld, name: nameof(NewWorld));
        MenuEtape = Serialize<byte>(MenuEtape, name: nameof(MenuEtape));

        FinBoss = Serialize<bool>(FinBoss, name: nameof(FinBoss));
        FinBossLevel = Serialize<FinBossLevel>(FinBossLevel, name: nameof(FinBossLevel));

        R2_Language = Serialize<int>(R2_Language, name: nameof(R2_Language));
        R2_ShowEngineInfo = Serialize<bool>(R2_ShowEngineInfo, name: nameof(R2_ShowEngineInfo));
        R2_DebugMode = Serialize<bool>(R2_DebugMode, name: nameof(R2_DebugMode));

        R2_UnusedMapLoopFunctionCall = Serialize<uint>(R2_UnusedMapLoopFunctionCall, name: nameof(R2_UnusedMapLoopFunctionCall));
        R2_MapInitFunctionCall = Serialize<uint>(R2_MapInitFunctionCall, name: nameof(R2_MapInitFunctionCall));

        GBA_EnableMultiplayerMenus = Serialize<bool>(GBA_EnableMultiplayerMenus, name: nameof(GBA_EnableMultiplayerMenus));
        GBA_MultiplayerTimeout = Serialize<ushort>(GBA_MultiplayerTimeout, name: nameof(GBA_MultiplayerTimeout));
        GBA_MultiplayerLevelLoad = Serialize<ushort>(GBA_MultiplayerLevelLoad, name: nameof(GBA_MultiplayerLevelLoad));
        GBA_MultiplayerPlayerSelection = Serialize<bool>(GBA_MultiplayerPlayerSelection, name: nameof(GBA_MultiplayerPlayerSelection));
        GBA_MultiplayerLevelSelection = Serialize<byte>(GBA_MultiplayerLevelSelection, name: nameof(GBA_MultiplayerLevelSelection));
    }
}