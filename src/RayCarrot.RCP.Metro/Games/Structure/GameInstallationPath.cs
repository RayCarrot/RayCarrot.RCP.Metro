using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Games.Structure;

// TODO: This can be expanded and improved upon a lot in the future. For example we could add
//       search patterns, allow files outside the install dir etc.
[JsonObject] // Force it to be serialized as an object even though it implements IEnumerable
public abstract class GameInstallationPath : IEnumerable<GameInstallationPath>
{
    protected GameInstallationPath(string path, GameInstallationPathType type, bool required)
    {
        Path = path;
        Type = type;
        Required = required;
    }

    public List<GameInstallationPath> SubPaths { get; } = new();
    public string Path { get; }
    public GameInstallationPathType Type { get; }
    public bool Required { get; }

    public abstract bool IsValid(FileSystemPath fullPath);

    public void Add(GameInstallationPath path) => SubPaths.Add(path);
    public void AddRange(IEnumerable<GameInstallationPath> paths) => SubPaths.AddRange(paths);

    public IEnumerator<GameInstallationPath> GetEnumerator() => SubPaths.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}