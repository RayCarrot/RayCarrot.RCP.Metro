namespace RayCarrot.RCP.Metro.Games.Finder;

public class WindowsPackageFinderQuery : FinderQuery
{
    public WindowsPackageFinderQuery(string packageName)
    {
        PackageName = packageName;
    }

    public string PackageName { get; }
}