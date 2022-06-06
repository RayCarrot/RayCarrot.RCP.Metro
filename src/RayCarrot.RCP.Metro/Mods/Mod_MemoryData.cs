using System;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Mod_MemoryData
{
    public Pointer? Offset { get; set; }
    public bool PendingChange { get; set; }

    protected abstract void SerializeImpl(SerializerObject s);

    public void Serialize(Context context)
    {
        if (Offset == null)
            throw new Exception("Offset is null");

        SerializerObject s = PendingChange ? context.Serializer : context.Deserializer;

        s.Goto(Offset);

        SerializeImpl(s);

        PendingChange = false;
    }
}