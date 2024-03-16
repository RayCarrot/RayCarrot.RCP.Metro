using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public abstract class MemoryData
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly HashSet<string> _modifiedValues = new();
    private readonly Dictionary<string, Pointer> _pointers = new();
    private Context? _context;

    private SerializerObject? GetSerializerObject(string name)
    {
        if (!_pointers.ContainsKey(name))
            return null;

        SerializerObject s = _modifiedValues.Remove(name) ? _context!.Serializer : _context!.Deserializer;
        s.Goto(_pointers[name]);
        return s;
    }

    protected T Serialize<T>(T obj, string name)
        where T : struct
    {
        SerializerObject? s = GetSerializerObject(name);

        if (s == null)
            return obj;

        return s.Serialize<T>(obj, name: name);
    }

    protected T[]? SerializeArray<T>(T[]? obj, long count, string name)
        where T : struct
    {
        SerializerObject? s = GetSerializerObject(name);

        if (s == null)
            return obj;

        return s.SerializeArray<T>(obj, count, name: name);
    }

    protected T? SerializeObject<T>(T? obj, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        SerializerObject? s = GetSerializerObject(name);

        if (s == null)
            return obj;

        return s.SerializeObject<T>(obj, onPreSerialize, name: name);
    }

    protected T[]? SerializeObjectArray<T>(T[]? obj, long count, string name, Action<T>? onPreSerialize = null) 
        where T : BinarySerializable, new()
    {
        SerializerObject? s = GetSerializerObject(name);

        if (s == null)
            return obj;

        return s.SerializeObjectArray<T>(obj, count, onPreSerialize, name: name);
    }

    protected string? SerializeString(string? obj, long length, string name)
    {
        SerializerObject? s = GetSerializerObject(name);

        if (s == null)
            return obj;

        return s.SerializeString(obj, length, name: name);
    }
    protected abstract bool ValidateImpl();
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
        {
            BinaryFile? file = firstMemFile.GetPointerFile(off.Value);

            if (file == null)
            {
                Logger.Warn("Failed to add offset {0}", off.Key);
                continue;
            }

            _pointers.Add(off.Key, new Pointer(off.Value, file));
        }
    }

    public bool Validate()
    {
        if (_context == null)
            throw new Exception("Attempted to validate memory data before initializing it");

        return ValidateImpl();
    }

    public void Serialize()
    {
        if (_context == null)
            throw new Exception("Attempted to serialize memory data before initializing it");

        SerializeImpl();
    }
}