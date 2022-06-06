using BinarySerializer;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Mod_R1_MemoryData
{
    public Mod_R1_MemoryData(Pointer offset)
    {
        Offset = offset;
    }

    public Pointer Offset { get; }

    public bool PendingChange { get; set; }

    public StatusBar? StatusBar { get; set; }
    public Poing? Poing { get; set; }

    public RayMode RayMode { get; set; }
    public RayEvts RayEvts { get; set; }

    public int MapTime { get; set; }
    public short XMap { get; set; }
    public short YMap { get; set; }

    public void Serialize(Context context)
    {
        SerializerObject s = PendingChange ? context.Serializer : context.Deserializer;

        s.Goto(Offset);

        // TODO-UPDATE: Don't hard-code offsets. Support multiple versions (this is for 1.21).
        s.DoAt(Offset + 0x16FF52, () => StatusBar = s.SerializeObject<StatusBar?>(StatusBar, name: nameof(StatusBar)));
        s.DoAt(Offset + 0x16F770, () => Poing = s.SerializeObject<Poing?>(Poing, name: nameof(Poing)));

        s.DoAt(Offset + 0x170868, () => RayMode = s.Serialize<RayMode>(RayMode, name: nameof(RayMode)));
        s.DoAt(Offset + 0x17081A, () => RayEvts = s.Serialize<RayEvts>(RayEvts, name: nameof(RayEvts)));

        s.DoAt(Offset + 0x16E8C0, () => MapTime = s.Serialize<int>(XMap, name: nameof(XMap)));
        s.DoAt(Offset + 0x17089E, () => XMap = s.Serialize<short>(XMap, name: nameof(XMap)));
        s.DoAt(Offset + 0x1708A6, () => YMap = s.Serialize<short>(YMap, name: nameof(YMap)));

        PendingChange = false;
    }
}