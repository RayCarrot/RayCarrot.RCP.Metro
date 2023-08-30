namespace RayCarrot.RCP.Metro.GameBanana;

/// <summary>
/// Provides access to the GameBanana API
/// </summary>
public class GameBananaApi
{
    private static string ApiRoot => "https://gamebanana.com/apiv11";

    public int RaymanControlPanelToolId => 10372;

    public string GetGameSubfeedUrl(int gameId, int page, string sort = "new")
    {
        return $"{ApiRoot}/Game/{gameId}/Subfeed?" +
               $"_nPage={page}&" +
               $"_sSort={sort}&" +
               $"_csvModelInclusions=Mod"; // Hard-code to only include mods for now - can add param for it later if needed
    }

    public string GetModUrl(int modId)
    {
        return $"{ApiRoot}/Mod/{modId}/ProfilePage";

        // Can alternatively do something like this to only get certain properties. Is it worth it?
        //return $"{ApiRoot}/Mod/{modId}?_csvProperties=_sName,_sDownloadUrl,_aFiles,_bIsTrashed,_nDownloadCount";
    }
}