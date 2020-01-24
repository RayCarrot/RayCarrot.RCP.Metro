namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Commons URLs used in the Rayman Control Panel
    /// </summary>
    public static class CommonUrls
    {
        #region Base

        /// <summary>
        /// The base URL
        /// </summary>
        public const string BaseUrl = "http://raycarrot.ylemnova.com/RCP/";

        /// <summary>
        /// The update manifest URL
        /// </summary>
        public const string UpdateManifestUrl = BaseUrl + "RCP_Metro_Manifest.json";

        /// <summary>
        /// The base resource URL
        /// </summary>
        public const string BaseResourceUrl = BaseUrl + "Resources/9.0.0/";

        /// <summary>
        /// The base URL for downloading utilities
        /// </summary>
        public const string UtilityBaseUrl = BaseResourceUrl + "Utilities/";

        /// <summary>
        /// The base URL for downloading games
        /// </summary>
        public const string GamesBaseUrl = BaseResourceUrl + "Games/";

        /// <summary>
        /// The base URL for downloading demos
        /// </summary>
        public const string DemosBaseUrl = BaseResourceUrl + "Demos/";

        #endregion

        #region Games

        /// <summary>
        /// The Rayman 3 Print Studio part 1 download URL
        /// </summary>
        public const string Games_PrintStudio1_Url = GamesBaseUrl + "PrintStudio1.zip";

        /// <summary>
        /// The Rayman 3 Print Studio part 2 download URL
        /// </summary>
        public const string Games_PrintStudio2_Url = GamesBaseUrl + "PrintStudio2.zip";

        /// <summary>
        /// The Globox Moment download URL
        /// </summary>
        public const string Games_GloboxMoment_Url = GamesBaseUrl + "GloboxMoment.zip";

        /// <summary>
        /// The Rayman The Dark Magician's Reign of Terror download URL
        /// </summary>
        public const string Games_TheDarkMagiciansReignofTerror_Url = GamesBaseUrl + "TheDarkMagiciansReignofTerror.zip";

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
        
        #endregion

        #region Utilities

        /// <summary>
        /// The Rayman 1 TPLS utility URL
        /// </summary>
        public const string R1_TPLS_Url = UtilityBaseUrl + "R1/TPLS.zip";

        /// <summary>
        /// The Rayman 1 complete soundtrack utility URL
        /// </summary>
        public const string R1_CompleteOST_URL = UtilityBaseUrl + "R1/CompleteOST.zip";

        /// <summary>
        /// The Rayman 1 incomplete soundtrack utility URL
        /// </summary>
        public const string R1_IncompleteOST_URL = UtilityBaseUrl + "R1/IncompleteOST.zip";

        /// <summary>
        /// The Rayman Designer CLIENT.EXE replacement URL
        /// </summary>
        public const string RD_ClientExe_URL = UtilityBaseUrl + "R1/RayKit/CLIENT.EXE";

        /// <summary>
        /// The Rayman Designer STARTUP.EXE replacement URL
        /// </summary>
        public const string RD_StartupExe_URL = UtilityBaseUrl + "R1/RayKit/STARTUP.EXE";

        /// <summary>
        /// The Rayman Designer RAYRUN.EXE replacement URL
        /// </summary>
        public const string RD_RayrunExe_URL = UtilityBaseUrl + "R1/RayKit/RAYRUN.EXE";

        /// <summary>
        /// The Rayman Designer English MAPPER.EXE replacement URL
        /// </summary>
        public const string RD_USMapperExe_URL = UtilityBaseUrl + "R1/RayKit/US/MAPPER.EXE";

        /// <summary>
        /// The Rayman Designer French MAPPER.EXE replacement URL
        /// </summary>
        public const string RD_FRMapperExe_URL = UtilityBaseUrl + "R1/RayKit/FR/MAPPER.EXE";

        /// <summary>
        /// The Rayman Designer German MAPPER.EXE replacement URL
        /// </summary>
        public const string RD_ALMapperExe_URL = UtilityBaseUrl + "R1/RayKit/AL/MAPPER.EXE";

        /// <summary>
        /// The Rayman 2 original fix.sna URL
        /// </summary>
        public const string R2_OriginalFixSna_URL = UtilityBaseUrl + "R2/Translation_Original/Fix.sna";

        /// <summary>
        /// The Rayman 2 original textures.cnt URL
        /// </summary>
        public const string R2_OriginalTexturesCnt_URL = UtilityBaseUrl + "R2/Translation_Original/Textures.cnt";

        /// <summary>
        /// The Rayman 2 Irish fix.sna URL
        /// </summary>
        public const string R2_IrishFixSna_URL = UtilityBaseUrl + "R2/Translation_Irish/Fix.sna";

        /// <summary>
        /// The Rayman 2 Swedish fix.sna URL
        /// </summary>
        public const string R2_SwedishFixSna_URL = UtilityBaseUrl + "R2/Translation_Swedish/Fix.sna";

        /// <summary>
        /// The Rayman 2 Swedish textures.cnt URL
        /// </summary>
        public const string R2_SwedishTexturesCnt_URL = UtilityBaseUrl + "R2/Translation_Swedish/Textures.cnt";

        /// <summary>
        /// The Rayman 2 Portuguese fix.sna URL
        /// </summary>
        public const string R2_PortugueseFixSna_URL = UtilityBaseUrl + "R2/Translation_Portuguese/Fix.sna";

        /// <summary>
        /// The Rayman 2 Portuguese textures.cnt URL
        /// </summary>
        public const string R2_PortugueseTexturesCnt_URL = UtilityBaseUrl + "R2/Translation_Portuguese/Textures.cnt";

        /// <summary>
        /// The Rayman 2 Slovak fix.sna URL
        /// </summary>
        public const string R2_SlovakFixSna_URL = UtilityBaseUrl + "R2/Translation_Slovak/Fix.sna";

        /// <summary>
        /// The Rayman 2 Slovak textures.cnt URL
        /// </summary>
        public const string R2_SlovakTexturesCnt_URL = UtilityBaseUrl + "R2/Translation_Slovak/Textures.cnt";

        /// <summary>
        /// The Rayman Origins original videos URL
        /// </summary>
        public const string RO_OriginalVideos_URL = UtilityBaseUrl + "RO/OriginalVideos.zip";

        /// <summary>
        /// The Rayman Origins high quality videos URL
        /// </summary>
        public const string RO_HQVideos_URL = UtilityBaseUrl + "RO/HQVideos.zip";

        /// <summary>
        /// The Rayman Origins updater URL
        /// </summary>
        public const string RO_Updater_URL = UtilityBaseUrl + "RO/Updater.zip";

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
        /// The Discord URL
        /// </summary>
        public const string DiscordUrl = "https://discord.gg/Cuq6wpX";

        /// <summary>
        /// The Steam group URL
        /// </summary>
        public const string SteamUrl = "https://steamcommunity.com/groups/RaymanControlPanel";

        /// <summary>
        /// The translation contribution URL
        /// </summary>
        public const string TranslationUrl = "https://steamcommunity.com/groups/RaymanControlPanel/discussions/0/1812044473314212117/";

        #endregion
    }
}