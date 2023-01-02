namespace RayCarrot.RCP.Metro.Games.Finder;

public class Win32ShortcutFinderQuery : FinderQuery
{
    public Win32ShortcutFinderQuery(string name)
    {
        Name = name;
    }

    public string Name { get; }
}