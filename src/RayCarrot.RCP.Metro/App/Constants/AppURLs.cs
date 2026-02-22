namespace RayCarrot.RCP.Metro;

// TODO: Refactor this. Don't use constant strings, but rather turn into a service class in DI. This way we can easily
//       change the URLs for testing, or even have local ones.

/// <summary>
/// Commons URLs used in the Rayman Control Panel
/// </summary>
public static class AppURLs
{
    #region GitHub

    public const string GitHubUserName = "RayCarrot";
    public const string GitHubRepoName = "RayCarrot.RCP.Metro";

    public const string GitHubReleaseExeFileName = "RaymanControlPanel.exe";
    public const string GitHubReleaseChangelogFileName = "Changelog.txt";

    public const string GitHubHostedNewsFilePath = "hosted/news.json";

    /// <summary>
    /// The GitHub repository URL
    /// </summary>
    public const string GitHubRepoUrl = $"https://github.com/{GitHubUserName}/{GitHubRepoName}";

    /// <summary>
    /// The Mod Loader wiki documentation URL
    /// </summary>
    public const string ModLoaderDocumentationUrl = $"{GitHubRepoUrl}/wiki/Mod-Loader";

    /// <summary>
    /// The latest GitHub release URL
    /// </summary>
    public const string LatestGitHubReleaseUrl = $"{GitHubRepoUrl}/releases/latest";

    #endregion

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
    /// The base resource URL
    /// </summary>
    public const string BaseResourceUrl = RCPBaseUrl + "resources/14.2.0/";

    /// <summary>
    /// The base URL for downloading setup files
    /// </summary>
    public const string SetupBaseUrl = BaseResourceUrl + "setup/";

    /// <summary>
    /// The base URL for downloading tools
    /// </summary>
    public const string ToolsBaseUrl = BaseResourceUrl + "tools/";

    #endregion

    #region Setup

    /// <summary>
    /// The Rayman 1 complete soundtrack utility URL
    /// </summary>
    public const string R1_CompleteOST_URL = SetupBaseUrl + "rayman1_complete_ost.zip";

    /// <summary>
    /// The Rayman Designer clean files URL
    /// </summary>
    public const string RD_CleanFiles_URL = SetupBaseUrl + "raymandesigner_clean.zip";

    #endregion

    #region Tools

    public const string PerLevelSoundtrackTool_URL = ToolsBaseUrl + "rayman1_per_level_soundtrack/tpls-tsr-3.1.1.zip";

    public const string RRR_PatchedBF_Steam_URL = ToolsBaseUrl + "raymanravingrabbids_prototype_restoration/RRR_Patched_Steam.zip";
    public const string RRR_PatchedBF_GOG_URL = ToolsBaseUrl + "raymanravingrabbids_prototype_restoration/RRR_Patched_GOG.zip";

    #endregion

    #region Mod Loader

    /// <summary>
    /// The GameBanana featured mods list URL
    /// </summary>
    public const string ModLoader_FeaturedGameBananaMods_URL = RCPBaseUrl + "featured_gb_mods.jsonc";

    #endregion

    #region Contact

    /// <summary>
    /// The YouTube URL
    /// </summary>
    public const string YouTubeUrl = "https://www.youtube.com/c/RayCarrot";

    /// <summary>
    /// The Bluesky URL
    /// </summary>
    public const string BlueskyUrl = "https://bsky.app/profile/raycarrot.bsky.social";

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