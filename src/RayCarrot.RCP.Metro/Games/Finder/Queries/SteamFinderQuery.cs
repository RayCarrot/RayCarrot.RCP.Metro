namespace RayCarrot.RCP.Metro.Games.Finder;

public class SteamFinderQuery : FinderQuery
{
    public SteamFinderQuery(string steamId)
    {
        SteamId = steamId;
    }

    public string SteamId { get; }
}