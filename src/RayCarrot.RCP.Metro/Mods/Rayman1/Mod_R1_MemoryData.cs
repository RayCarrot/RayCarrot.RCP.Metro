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
    public short ActiveObjCount { get; set; }
    public short XMap { get; set; }
    public short YMap { get; set; }

    public bool AllWorld { get; set; }
    public short NumLevelChoice { get; set; }
    public short NumWorldChoice { get; set; }
    public short NumLevel { get; set; }
    public short NumWorld { get; set; }
    public short NewWorld { get; set; }
    public byte MenuEtape { get; set; }

    public bool FinBoss { get; set; }
    public FinBossLevel FinBossLevel { get; set; }

    protected override void SerializeImpl(SerializerObject s)
    {
        // TODO-UPDATE: Don't hard-code offsets. Support multiple versions (this is for 1.21).
        s.DoAt(Offset + 0x16FF52, () => StatusBar = s.SerializeObject<StatusBar?>(StatusBar, name: nameof(StatusBar)));
        s.DoAt(Offset + 0x16F770, () => Poing = s.SerializeObject<Poing?>(Poing, name: nameof(Poing)));
        s.DoAt(Offset + 0x16F650, () => Ray = s.SerializeObject<ObjData?>(Ray, x => x!.Pre_IsSerializingFromMemory = true, name: nameof(Ray)));
        s.DoAt(Offset + 0x170898, () => HelicoTime = s.Serialize<short>(HelicoTime, name: nameof(HelicoTime)));

        s.DoAt(Offset + 0x135D5C, () => WorldInfo = s.SerializeObjectArray<WorldInfo>(WorldInfo, 24, name: nameof(WorldInfo)));
        s.DoAt(Offset + 0x18648C, () => RandomIndex = s.Serialize<byte>(RandomIndex, name: nameof(RandomIndex)));

        s.DoAt(Offset + 0x170868, () => RayMode = s.Serialize<RayMode>(RayMode, name: nameof(RayMode)));
        s.DoAt(Offset + 0x170A73, () => RayModeSpeed = s.Serialize<byte>(RayModeSpeed, name: nameof(RayModeSpeed)));
        s.DoAt(Offset + 0x17081A, () => RayEvts = s.Serialize<RayEvts>(RayEvts, name: nameof(RayEvts)));

        s.DoAt(Offset + 0x16E8C0, () => MapTime = s.Serialize<int>(XMap, name: nameof(XMap)));
        s.DoAt(Offset + 0x170024, () => ActiveObjCount = s.Serialize<short>(ActiveObjCount, name: nameof(ActiveObjCount)));
        s.DoAt(Offset + 0x17089E, () => XMap = s.Serialize<short>(XMap, name: nameof(XMap)));
        s.DoAt(Offset + 0x1708A6, () => YMap = s.Serialize<short>(YMap, name: nameof(YMap)));

        s.DoAt(Offset + 0x170A61, () => AllWorld = s.Serialize<bool>(AllWorld, name: nameof(AllWorld)));
        s.DoAt(Offset + 0x17082E, () => NumLevelChoice = s.Serialize<short>(NumLevelChoice, name: nameof(NumLevelChoice)));
        s.DoAt(Offset + 0x17083A, () => NumWorldChoice = s.Serialize<short>(NumWorldChoice, name: nameof(NumWorldChoice)));
        s.DoAt(Offset + 0x17087C, () => NumLevel = s.Serialize<short>(NumLevel, name: nameof(NumLevel)));
        s.DoAt(Offset + 0x17088C, () => NumWorld = s.Serialize<short>(NumWorld, name: nameof(NumWorld)));
        s.DoAt(Offset + 0x170892, () => NewWorld = s.Serialize<short>(NewWorld, name: nameof(NewWorld)));
        s.DoAt(Offset + 0x170A37, () => MenuEtape = s.Serialize<byte>(MenuEtape, name: nameof(MenuEtape)));

        s.DoAt(Offset + 0x170A76, () => FinBoss = s.Serialize<bool>(FinBoss, name: nameof(FinBoss)));
        s.DoAt(Offset + 0x17081E, () => FinBossLevel = s.Serialize<FinBossLevel>(FinBossLevel, name: nameof(FinBossLevel)));
    }
}