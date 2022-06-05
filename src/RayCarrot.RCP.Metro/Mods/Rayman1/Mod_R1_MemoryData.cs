#nullable disable
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

    public StatusBar StatusBar { get; set; }
    public RayMode RayMode { get; set; }
    public RayEvts RayEvts { get; set; }

    public void Serialize(Context context)
    {
        SerializerObject s = PendingChange ? context.Serializer : context.Deserializer;

        s.Goto(Offset);

        // TODO-UPDATE: Don't hard-code offsets. Support multiple versions (this is for 1.21).
        s.DoAt(Offset + 0x16FF52, () => StatusBar = s.SerializeObject<StatusBar>(StatusBar, name: nameof(StatusBar)));
        s.DoAt(Offset + 0x170868, () => RayMode = s.Serialize<RayMode>(RayMode, name: nameof(RayMode)));
        s.DoAt(Offset + 0x17081A, () => RayEvts = s.Serialize<RayEvts>(RayEvts, name: nameof(RayEvts)));

        PendingChange = false;
    }
}