namespace RayCarrot.RCP.Metro;

public static class SteamHelpers
{
    public static string GetStorePageURl(string steamID)
    {
        return $"https://store.steampowered.com/app/{steamID}";
    }
}