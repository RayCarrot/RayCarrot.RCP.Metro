using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Structure;

// TODO: This can be expanded and improved upon a lot in the future. For example we could add
//       search patterns, allow files outside the install dir etc.
[JsonObject] // Force it to be serialized as an object even though it implements IEnumerable
public abstract class ProgramPath : IEnumerable<ProgramPath>
{
    protected ProgramPath(string path, ProgramPathType type, bool required)
    {
        Path = path;
        Type = type;
        Required = required;
    }

    public List<ProgramPath> SubPaths { get; } = new();
    public string Path { get; }
    public ProgramPathType Type { get; }
    public bool Required { get; }

    public abstract bool IsValid(IFileSystemSource source, string fullPath);

    public void Add(ProgramPath path) => SubPaths.Add(path);
    public void AddRange(IEnumerable<ProgramPath> paths) => SubPaths.AddRange(paths);

    public IEnumerator<ProgramPath> GetEnumerator() => SubPaths.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}