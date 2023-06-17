namespace RayCarrot.RCP.Metro;

public static class UbisoftConnectHelpers
{
    public static string GetGameLaunchURI(string gameId)
    {
        return $@"uplay://launch/{gameId}/0";
    }
}