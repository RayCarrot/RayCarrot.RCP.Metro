namespace RayCarrot.RCP.Metro;

public static class SteamHelpers
{
    public static string GetStorePageURL(string steamId)
    {
        return $"https://store.steampowered.com/app/{steamId}";
    }

    public static string GetCommunityPageURL(string steamId)
    {
        return $"https://steamcommunity.com/app/{steamId}";
    }

    public static string GetGameLaunchURI(string steamId)
    {
        return $@"steam://rungameid/{steamId}";
    }

    public static string GetGameLaunchURI(string steamId, string arguments)
    {
        return $@"steam://run/{steamId}/{arguments}";
    }
}