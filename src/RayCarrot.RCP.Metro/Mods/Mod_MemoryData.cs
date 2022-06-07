using System;
using System.Collections.Generic;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Mod_MemoryData
{
    public Pointer? Offset { get; set; }
    public Dictionary<string, long>? Offsets { get; set; }
    public bool PendingChange { get; set; } // TODO-UPDATE: Change to HashSet<string> for the modified properties to avoid writing everything?

    protected T Serialize<T>(SerializerObject s, T obj, string name)
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        s.Goto(Offset + Offsets[name]);
        return s.Serialize<T>(obj, name: name);
    }

    protected T[]? SerializeArray<T>(SerializerObject s, T[]? obj, long count, string name)
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        s.Goto(Offset + Offsets[name]);
        return s.SerializeArray<T>(obj, count, name: name);
    }

    protected T? SerializeObject<T>(SerializerObject s, T? obj, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        s.Goto(Offset + Offsets[name]);
        return s.SerializeObject<T>(obj, onPreSerialize, name: name);
    }

    protected T[]? SerializeObjectArray<T>(SerializerObject s, T[]? obj, long count, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        s.Goto(Offset + Offsets[name]);
        return s.SerializeObjectArray<T>(obj, count, onPreSerialize, name: name);
    }

    protected abstract void SerializeImpl(SerializerObject s);

    public bool SupportsProperty(string name) => Offsets?.ContainsKey(name) == true;

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