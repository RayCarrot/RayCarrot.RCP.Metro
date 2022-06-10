using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public abstract class Mod_MemoryData
{
    private readonly HashSet<string> _modifiedValues = new();
    private readonly Dictionary<string, Pointer> _pointers = new();
    private Context? _context;

    protected T Serialize<T>(T obj, string name)
    {
        if (!_pointers.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? _context!.Serializer : _context!.Deserializer;
        s.Goto(_pointers[name]);
        return s.Serialize<T>(obj, name: name);
    }

    protected T[]? SerializeArray<T>(T[]? obj, long count, string name)
    {
        if (!_pointers.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? _context!.Serializer : _context!.Deserializer;
        s.Goto(_pointers[name]);
        return s.SerializeArray<T>(obj, count, name: name);
    }

    protected T? SerializeObject<T>(T? obj, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        if (!_pointers.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? _context!.Serializer : _context!.Deserializer;
        s.Goto(_pointers[name]);
        return s.SerializeObject<T>(obj, onPreSerialize, name: name);
    }

    protected T[]? SerializeObjectArray<T>(T[]? obj, long count, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        if (!_pointers.ContainsKey(name))
            return obj;

        SerializerObject s = _modifiedValues.Remove(name) ? _context!.Serializer : _context!.Deserializer;
        s.Goto(_pointers[name]);
        return s.SerializeObjectArray<T>(obj, count, onPreSerialize, name: name);
    }

    protected abstract void SerializeImpl();

    public bool SupportsProperty(string name) => _pointers.ContainsKey(name);

    public void ModifiedValue(string propertyName) => _modifiedValues.Add(propertyName);
    public void ModifiedValue(params string[] propertyNames)
    {
        foreach (string prop in propertyNames)
            _modifiedValues.Add(prop);
    }

    public void ClearModifiedValues() => _modifiedValues.Clear();

    public void Initialize(Context context, Dictionary<string, long>? offsets)
    {
        _context = context;
        _pointers.Clear();

        if (offsets == null)
            return;

        BinaryFile firstMemFile = _context.MemoryMap.Files.First(x => x is ProcessMemoryStreamFile);

        foreach (var off in offsets)
            _pointers.Add(off.Key, new Pointer(off.Value, firstMemFile.GetPointerFile(off.Value)));
    }

    public void Serialize()
    {
        if (_context == null)
            throw new Exception("Attempted to serialize memory data before initializing it");

        SerializeImpl();
    }
}