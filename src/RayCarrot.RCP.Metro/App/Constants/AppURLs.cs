using System;

namespace RayCarrot.RCP.Metro;

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
    /// The Rayman 1 TPLS utility URL
    /// </summary>
    public const string R1_TPLS_Url = UtilityBaseUrl + "r1/TPLS.zip";

    /// <summary>
    /// The Rayman 1 complete soundtrack utility URL
    /// </summary>
    public const string R1_CompleteOST_URL = UtilityBaseUrl + "r1/CompleteOST.zip";

    /// <summary>
    /// The Rayman 1 incomplete soundtrack utility URL
    /// </summary>
    public const string R1_IncompleteOST_URL = UtilityBaseUrl + "r1/IncompleteOST.zip";

    /// <summary>
    /// The Rayman Designer CLIENT.EXE replacement URL
    /// </summary>
    public const string RD_ClientExe_URL = UtilityBaseUrl + "r1/raykit/CLIENT.EXE";

    /// <summary>
    /// The Rayman Designer STARTUP.EXE replacement URL
    /// </summary>
    public const string RD_StartupExe_URL = UtilityBaseUrl + "r1/raykit/STARTUP.EXE";

    /// <summary>
    /// The Rayman Designer RAYRUN.EXE replacement URL
    /// </summary>
    public const string RD_RayrunExe_URL = UtilityBaseUrl + "r1/raykit/RAYRUN.EXE";

    /// <summary>
    /// The Rayman Designer English MAPPER.EXE replacement URL
    /// </summary>
    public const string RD_USMapperExe_URL = UtilityBaseUrl + "r1/raykit/us/MAPPER.EXE";

    /// <summary>
    /// The Rayman Designer French MAPPER.EXE replacement URL
    /// </summary>
    public const string RD_FRMapperExe_URL = UtilityBaseUrl + "r1/raykit/fr/MAPPER.EXE";

    /// <summary>
    /// The Rayman Designer German MAPPER.EXE replacement URL
    /// </summary>
    public const string RD_ALMapperExe_URL = UtilityBaseUrl + "r1/raykit/al/MAPPER.EXE";

    /// <summary>
    /// The Rayman 2 original fix.sna URL
    /// </summary>
    public const string R2_OriginalFixSna_URL = UtilityBaseUrl + "r2/translation_original/Fix.sna";

    /// <summary>
    /// The Rayman 2 original textures.cnt URL
    /// </summary>
    public const string R2_OriginalTexturesCnt_URL = UtilityBaseUrl + "r2/translation_original/Textures.cnt";

    /// <summary>
    /// The Rayman 2 Irish fix.sna URL
    /// </summary>
    public const string R2_IrishFixSna_URL = UtilityBaseUrl + "r2/translation_irish/Fix.sna";

    /// <summary>
    /// The Rayman 2 Swedish fix.sna URL
    /// </summary>
    public const string R2_SwedishFixSna_URL = UtilityBaseUrl + "r2/translation_swedish/Fix.sna";

    /// <summary>
    /// The Rayman 2 Swedish textures.cnt URL
    /// </summary>
    public const string R2_SwedishTexturesCnt_URL = UtilityBaseUrl + "r2/translation_swedish/Textures.cnt";

    /// <summary>
    /// The Rayman 2 Portuguese fix.sna URL
    /// </summary>
    public const string R2_PortugueseFixSna_URL = UtilityBaseUrl + "r2/translation_portuguese/Fix.sna";

    /// <summary>
    /// The Rayman 2 Portuguese textures.cnt URL
    /// </summary>
    public const string R2_PortugueseTexturesCnt_URL = UtilityBaseUrl + "r2/translation_portuguese/Textures.cnt";

    /// <summary>
    /// The Rayman 2 Slovak fix.sna URL
    /// </summary>
    public const string R2_SlovakFixSna_URL = UtilityBaseUrl + "r2/translation_slovak/Fix.sna";

    /// <summary>
    /// The Rayman 2 Slovak textures.cnt URL
    /// </summary>
    public const string R2_SlovakTexturesCnt_URL = UtilityBaseUrl + "r2/translation_slovak/Textures.cnt";

    /// <summary>
    /// The Rayman Origins original videos URL
    /// </summary>
    public const string RO_OriginalVideos_URL = UtilityBaseUrl + "ro/OriginalVideos.zip";

    /// <summary>
    /// The Rayman Origins high quality videos URL
    /// </summary>
    public const string RO_HQVideos_URL = UtilityBaseUrl + "ro/HQVideos.zip";

    /// <summary>
    /// The Rayman Origins updater URL
    /// </summary>
    public const string RO_Updater_URL = UtilityBaseUrl + "ro/Updater.zip";

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