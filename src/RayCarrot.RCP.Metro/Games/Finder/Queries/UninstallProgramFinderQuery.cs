namespace RayCarrot.RCP.Metro.Games.Finder;

public class UninstallProgramFinderQuery : FinderQuery
{
    public UninstallProgramFinderQuery(string displayName)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }
}