﻿using System.Collections.Generic;
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
    public byte RayModeSpeed { get; set; } // IDEA: Add? This is for place ray.
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

    public int R2_Language { get; set; }
    public bool R2_ShowEngineInfo { get; set; }
    public bool R2_DebugMode { get; set; }
    public uint R2_UnusedMapLoopFunctionCall { get; set; }
    public uint R2_MapInitFunctionCall { get; set; } // Not unused, but it only resets the random index so not very important

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

        [nameof(R2_Language)] = 0x80145974,
        [nameof(R2_ShowEngineInfo)] = 0x801459b3,
        [nameof(R2_DebugMode)] = 0x80145a58,

        [nameof(R2_UnusedMapLoopFunctionCall)] = 0x801164c8,
        [nameof(R2_MapInitFunctionCall)] = 0x800e7c18,
    };

    protected override void SerializeImpl(Context context)
    {
        StatusBar = SerializeObject<StatusBar>(context, StatusBar, name: nameof(StatusBar));
        Poing = SerializeObject<Poing>(context, Poing, name: nameof(Poing));
        Ray = SerializeObject<ObjData>(context, Ray, onPreSerialize: x => x.Pre_IsSerializingFromMemory = true, name: nameof(Ray));
        R2_Ray = SerializeObject<R2_ObjData>(context, R2_Ray, onPreSerialize: x => x.Pre_IsSerializingFromMemory = true, name: nameof(R2_Ray));
        HelicoTime = Serialize<short>(context, HelicoTime, name: nameof(HelicoTime));

        WorldInfo = SerializeObjectArray<WorldInfo>(context, WorldInfo, 24, name: nameof(WorldInfo));
        RandomIndex = Serialize<byte>(context, RandomIndex, name: nameof(RandomIndex));

        RayMode = Serialize<RayMode>(context, RayMode, name: nameof(RayMode));
        RayModeSpeed = Serialize<byte>(context, RayModeSpeed, name: nameof(RayModeSpeed));
        RayEvts = Serialize<RayEvts>(context, RayEvts, name: nameof(RayEvts));
        R2_RayEvts = Serialize<R2_RayEvts>(context, R2_RayEvts, name: nameof(R2_RayEvts));

        MapTime = Serialize<int>(context, MapTime, name: nameof(MapTime));
        ActiveObjects = SerializeArray<short>(context, ActiveObjects, 112, name: nameof(ActiveObjects));
        XMap = Serialize<short>(context, XMap, name: nameof(XMap));
        YMap = Serialize<short>(context, YMap, name: nameof(YMap));

        AllWorld = Serialize<bool>(context, AllWorld, name: nameof(AllWorld));
        NumLevelChoice = Serialize<short>(context, NumLevelChoice, name: nameof(NumLevelChoice));
        NumWorldChoice = Serialize<short>(context, NumWorldChoice, name: nameof(NumWorldChoice));
        NumLevel = Serialize<short>(context, NumLevel, name: nameof(NumLevel));
        NumWorld = Serialize<short>(context, NumWorld, name: nameof(NumWorld));
        NewLevel = Serialize<short>(context, NewLevel, name: nameof(NewLevel));
        NewWorld = Serialize<short>(context, NewWorld, name: nameof(NewWorld));
        MenuEtape = Serialize<byte>(context, MenuEtape, name: nameof(MenuEtape));

        FinBoss = Serialize<bool>(context, FinBoss, name: nameof(FinBoss));
        FinBossLevel = Serialize<FinBossLevel>(context, FinBossLevel, name: nameof(FinBossLevel));

        R2_Language = Serialize<int>(context, R2_Language, name: nameof(R2_Language));
        R2_ShowEngineInfo = Serialize<bool>(context, R2_ShowEngineInfo, name: nameof(R2_ShowEngineInfo));
        R2_DebugMode = Serialize<bool>(context, R2_DebugMode, name: nameof(R2_DebugMode));

        R2_UnusedMapLoopFunctionCall = Serialize<uint>(context, R2_UnusedMapLoopFunctionCall, name: nameof(R2_UnusedMapLoopFunctionCall));
        R2_MapInitFunctionCall = Serialize<uint>(context, R2_MapInitFunctionCall, name: nameof(R2_MapInitFunctionCall));
    }
}