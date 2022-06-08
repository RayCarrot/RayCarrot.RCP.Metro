using System;
using System.Collections.Generic;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Mod_MemoryData
{
    private readonly HashSet<string> _modifiedValues = new();

    public Pointer? Offset { get; set; }
    public Dictionary<string, long>? Offsets { get; set; }

    protected T Serialize<T>(Context context, T obj, string name)
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? context.Serializer : context.Deserializer;
        s.Goto(Offset + Offsets[name]);
        return s.Serialize<T>(obj, name: name);
    }

    protected T[]? SerializeArray<T>(Context context, T[]? obj, long count, string name)
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? context.Serializer : context.Deserializer;
        s.Goto(Offset + Offsets[name]);
        return s.SerializeArray<T>(obj, count, name: name);
    }

    protected T? SerializeObject<T>(Context context, T? obj, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? context.Serializer : context.Deserializer;
        s.Goto(Offset + Offsets[name]);
        return s.SerializeObject<T>(obj, onPreSerialize, name: name);
    }

    protected T[]? SerializeObjectArray<T>(Context context, T[]? obj, long count, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        if (Offsets == null)
            throw new Exception("Offsets table is null");

        if (!Offsets.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? context.Serializer : context.Deserializer;
        s.Goto(Offset + Offsets[name]);
        return s.SerializeObjectArray<T>(obj, count, onPreSerialize, name: name);
    }

    protected abstract void SerializeImpl(Context context);

    public bool SupportsProperty(string name) => Offsets?.ContainsKey(name) == true;

    public void ModifiedValue(string propertyName) => _modifiedValues.Add(propertyName);
    public void ModifiedValue(params string[] propertyNames)
    {
        foreach (string prop in propertyNames)
            _modifiedValues.Add(prop);
    }

    public void ClearModifiedValues() => _modifiedValues.Clear();

    public void Serialize(Context context)
    {
        if (Offset == null)
            throw new Exception("Offset is null");

        SerializeImpl(context);
    }
}