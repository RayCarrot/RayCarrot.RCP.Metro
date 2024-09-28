namespace RayCarrot.RCP.Metro;

// TODO: Refactor this. Don't use constant strings, but rather turn into a service class in DI. This way we can easily
//       change the URLs for testing, or even have local ones.

/// <summary>
/// Commons URLs used in the Rayman Control Panel
/// </summary>
public static class AppURLs
{
    #region Base

    /// <summary>
    /// The RCP base URL
    /// </summary>
    public const string BaseUrl = "https://raym.app/";

    /// <summary>
    /// The RCP base URL
    /// </summary>
    public const string RCPBaseUrl = BaseUrl + "rcp/";

    /// <summary>
    /// The update manifest URL
    /// </summary>
    public const string UpdateManifestUrl = RCPBaseUrl + "RCP_Metro_Manifest.json";

    /// <summary>
    /// The app news URL
    /// </summary>
    public const string AppNewsUrl = RCPBaseUrl + "news.json";

    /// <summary>
    /// The base resource URL
    /// </summary>
    public const string BaseResourceUrl = RCPBaseUrl + "resources/12.0.0/";

    /// <summary>
    /// The base URL for downloading utilities
    /// </summary>
    public const string UtilityBaseUrl = BaseResourceUrl + "utilities/";

    /// <summary>
    /// The base URL for downloading mods
    /// </summary>
    public const string ModsBaseUrl = BaseResourceUrl + "mods/";

    /// <summary>
    /// The base URL for downloading games
    /// </summary>
    public const string GamesBaseUrl = BaseResourceUrl + "games/";

    /// <summary>
    /// The base URL for downloading demos
    /// </summary>
    public const string DemosBaseUrl = BaseResourceUrl + "demos/";

    #endregion

    #region Games

    /// <summary>
    /// The Rayman 1 Minigames download URL
    /// </summary>
    public const string Games_Ray1Minigames_Url = GamesBaseUrl + "Ray1Minigames.zip";

    /// <summary>
    /// The Rayman 3 Print Studio part 1 download URL
    /// </summary>
    public const string Games_PrintStudio1_Url = GamesBaseUrl + "PrintStudio1.zip";

    /// <summary>
    /// The Rayman 3 Print Studio part 2 download URL
    /// </summary>
    public const string Games_PrintStudio2_Url = GamesBaseUrl + "PrintStudio2.zip";

    /// <summary>
    /// The Rayman Raving Rabbids Activity Center download URL
    /// </summary>
    public const string Games_RavingRabbidsActivityCenter_Url = GamesBaseUrl + "RavingRabbidsActivityCenter.zip";

    #endregion

    #region Demos

    /// <summary>
    /// The Rayman 3 Demo 1 download URL
    /// </summary>
    public const string Games_R3Demo1_Url = DemosBaseUrl + "R3_Demo_1.zip";

    /// <summary>
    /// The Rayman 3 Demo 2 download URL
    /// </summary>
    public const string Games_R3Demo2_Url = DemosBaseUrl + "R3_Demo_2.zip";

    /// <summary>
    /// The Rayman 3 Demo 3 download URL
    /// </summary>
    public const string Games_R3Demo3_Url = DemosBaseUrl + "R3_Demo_3.zip";

    /// <summary>
    /// The Rayman 3 Demo 4 download URL
    /// </summary>
    public const string Games_R3Demo4_Url = DemosBaseUrl + "R3_Demo_4.zip";

    /// <summary>
    /// The Rayman 3 Demo 5 download URL
    /// </summary>
    public const string Games_R3Demo5_Url = DemosBaseUrl + "R3_Demo_5.zip";
        
    /// <summary>
    /// The Rayman M Demo download URL
    /// </summary>
    public const string Games_RMDemo_Url = DemosBaseUrl + "Rayman_M_Demo.zip";

    /// <summary>
    /// The Rayman 2 Demo 1 download URL
    /// </summary>
    public const string Games_R2Demo1_Url = DemosBaseUrl + "Rayman_2_Demo_1.zip";
        
    /// <summary>
    /// The Rayman 2 Demo 2 download URL
    /// </summary>
    public const string Games_R2Demo2_Url = DemosBaseUrl + "Rayman_2_Demo_2.zip";
        
    /// <summary>
    /// The Rayman Gold Demo download URL
    /// </summary>
    public const string Games_RGoldDemo_Url = DemosBaseUrl + "Rayman_Gold_Demo.zip";
        
    /// <summary>
    /// The Rayman 1 Demo 1 download URL
    /// </summary>
    public const string Games_R1Demo1_Url = DemosBaseUrl + "Rayman_1_Demo_1.zip";
        
    /// <summary>
    /// The Rayman 1 Demo 2 download URL
    /// </summary>
    public const string Games_R1Demo2_Url = DemosBaseUrl + "Rayman_1_Demo_2.zip";

    /// <summary>
    /// The Rayman 1 Demo 3 download URL
    /// </summary>
    public const string Games_R1Demo3_Url = DemosBaseUrl + "Rayman_1_Demo_3.zip";

    /// <summary>
    /// The Rayman Raving Rabbids Demo download URL
    /// </summary>
    public const string Games_RRRDemo_Url = DemosBaseUrl + "RRR_Demo.zip";

    #endregion

    #region Utilities

    /// <summary>
    /// The Rayman 1 complete soundtrack utility URL
    /// </summary>
    public const string R1_CompleteOST_URL = UtilityBaseUrl + "r1/CompleteOST.zip";

    /// <summary>
    /// The Rayman Designer clean files URL
    /// </summary>
    public const string RD_CleanFiles_URL = UtilityBaseUrl + "r1/CleanRayKit.zip";

    #endregion

    #region Mods

    /// <summary>
    /// The Rayman Raving Rabbids patched Big File (Steam) URL
    /// </summary>
    public const string RRR_PatchedBF_Steam_URL = ModsBaseUrl + "rrr/RRR_Patched_Steam.zip";

    /// <summary>
    /// The Rayman Raving Rabbids patched Big File (GOG) URL
    /// </summary>
    public const string RRR_PatchedBF_GOG_URL = ModsBaseUrl + "rrr/RRR_Patched_GOG.zip";

    #endregion

    #region Mod Loader

    /// <summary>
    /// The GameBanana featured mods list URL
    /// </summary>
    public const string ModLoader_FeaturedGameBananaMods_URL = RCPBaseUrl + "featured_gb_mods.jsonc";

    #endregion

    #region Contact

    /// <summary>
    /// The GitHub project URL
    /// </summary>
    public const string GitHubUrl = "https://github.com/RayCarrot/Rayman-Control-Panel-Metro";

    /// <summary>
    /// The YouTube URL
    /// </summary>
    public const string YouTubeUrl = "https://www.youtube.com/c/RayCarrot";

    /// <summary>
    /// The Twitter URL
    /// </summary>
    public const string TwitterUrl = "https://twitter.com/RayCarrot";

    /// <summary>
    /// The email URL
    /// </summary>
    public const string EmailUrl = "mailto:RayCarrotMaster@gmail.com";

    /// <summary>
    /// The Steam group URL
    /// </summary>
    public const string SteamUrl = "https://steamcommunity.com/groups/RaymanControlPanel";

    /// <summary>
    /// The translation contribution URL
    /// </summary>
    public const string TranslationUrl = "https://steamcommunity.com/groups/RaymanControlPanel/discussions/0/1812044473314212117/";

    #endregion

    #region raym.app

    /// <summary>
    /// The base URL for RayMap
    /// </summary>
    public const string RayMapBaseUrl = BaseUrl + "maps/";

    /// <summary>
    /// The base URL for Ray1Map
    /// </summary>
    public const string Ray1MapBaseUrl = BaseUrl + "maps_r1/";

    /// <summary>
    /// Gets the game URL for RayMap
    /// </summary>
    /// <param name="mode">The game mode</param>
    /// <param name="folder">The game folder, as a relative path</param>
    /// <returns>The URL</returns>
    public static string GetRayMapGameURL(string mode, string folder) => $"{RayMapBaseUrl}?mode={mode}&folder={folder}";

    /// <summary>
    /// Gets the game URL for Ray1Map
    /// </summary>
    /// <param name="mode">The game mode</param>
    /// <param name="folder">The game folder, as a relative path</param>
    /// <param name="vol">Optional volume argument</param>
    /// <returns>The URL</returns>
    public static string GetRay1MapGameURL(string mode, string folder, string? vol = null) => $"{Ray1MapBaseUrl}?mode={mode}&folder={folder}{(vol != null ? $"&vol={vol}" : String.Empty)}";

    #endregion
}