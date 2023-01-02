namespace RayCarrot.RCP.Metro.Games.Finder;

public class UbiIniFinderQuery : FinderQuery
{
    public UbiIniFinderQuery(string sectionName)
    {
        SectionName = sectionName;
    }

    public string SectionName { get; }
}