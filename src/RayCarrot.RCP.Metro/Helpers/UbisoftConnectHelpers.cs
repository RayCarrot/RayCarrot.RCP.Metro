namespace RayCarrot.RCP.Metro;

public static class UbisoftConnectHelpers
{
    public static string GetStorePageURL(string productId)
    {
        return $"https://store.ubisoft.com/game?pid={productId}";
    }

    public static string GetGameLaunchURI(string gameId)
    {
        return $@"uplay://launch/{gameId}/0";
    }
}